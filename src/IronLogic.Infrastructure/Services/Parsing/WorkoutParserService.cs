using System.Globalization;
using System.Text.RegularExpressions;
using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Shared;

namespace IronLogic.Infrastructure.Services.Parsing;

/// <summary>
///     A service responsible for parsing raw workout text into a structured <see cref="ParsedWorkoutDto" />.
///     All weights are automatically normalized to kilograms (kg) for consistent storage.
/// </summary>
public partial class WorkoutParserService : IWorkoutParserService
{
    private const decimal LbsToKgConversionFactor = 0.45359237m;

    /// <summary>
    ///     Parses a raw string of workout data into a structured <see cref="ParsedWorkoutDto" />.
    ///     Automatically detects weight units (lbs or kg) and normalizes all weights to kilograms.
    /// </summary>
    /// <param name="rawText">The raw text representing the workout log.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the <see cref="ParsedWorkoutDto" /> on success,
    ///     or an error message on failure.
    /// </returns>
    public Result<ParsedWorkoutDto> Parse(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return Result.Failure<ParsedWorkoutDto>("Raw workout text is empty.");

        var headerMatch = HeaderRegex().Match(rawText);
        if (!headerMatch.Success)
            return Result.Failure<ParsedWorkoutDto>("Invalid workout header format. Check the title and date.");

        var parsedDto = new ParsedWorkoutDto
        {
            Title = headerMatch.Groups["title"].Value.Trim(),
            Exercises = [] // Initialization is best done in the DTO class itself.
        };

        var dateString =
            $"{headerMatch.Groups["month"].Value} {headerMatch.Groups["day"].Value} {headerMatch.Groups["year"].Value} {headerMatch.Groups["time"].Value}";
        if (!DateTime.TryParseExact(dateString, "MMM d yyyy h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var parsedDate))
            return Result.Failure<ParsedWorkoutDto>("Workout date or time is not readable.");

        parsedDto.Date = parsedDate;

        var exerciseMatches = ExerciseBlockRegex().Matches(rawText);
        if (exerciseMatches.Count == 0)
            return Result.Failure<ParsedWorkoutDto>("No exercises found in the text.");

        // Set initial capacity for the exercises list for better performance.
        parsedDto.Exercises.Capacity = exerciseMatches.Count;

        foreach (Match exerciseMatch in exerciseMatches)
        {
            var exerciseNameSpan = exerciseMatch.Groups["exerciseName"].ValueSpan.Trim();

            if (exerciseNameSpan.IsWhiteSpace())
                continue;

            var exerciseDto = new ParsedExerciseDto
            {
                Name = exerciseNameSpan.ToString()
            };

            var setsText = exerciseMatch.Groups["sets"].Value;
            var setMatches = SetRegex().Matches(setsText);

            // 🚀 Set initial capacity for the sets list based on the number of matches found.
            exerciseDto.Sets = new List<ParsedSetDto>(setMatches.Count);

            foreach (Match setMatch in setMatches)
            {
                var weightValue = setMatch.Groups["weight"].Success
                    ? decimal.Parse(setMatch.Groups["weight"].ValueSpan, CultureInfo.InvariantCulture)
                    : 0;

                var unit = setMatch.Groups["unit"].Success
                    ? setMatch.Groups["unit"].Value.ToLowerInvariant()
                    : "lbs";

                var normalizedWeight = NormalizeWeightToKg(weightValue, unit);

                exerciseDto.Sets.Add(new ParsedSetDto
                {
                    // 🚀 Use ValueSpan to avoid string allocation.
                    SetIndex = int.Parse(setMatch.Groups["setIndex"].ValueSpan),

                    Weight = normalizedWeight,

                    Reps = int.Parse(setMatch.Groups["reps"].ValueSpan),

                    Rpe = setMatch.Groups["rpe"].Success
                        ? decimal.Parse(setMatch.Groups["rpe"].ValueSpan, CultureInfo.InvariantCulture)
                        : null
                });
            }

            // Using Count > 0 is more performant than Any().
            if (exerciseDto.Sets.Count > 0)
                parsedDto.Exercises.Add(exerciseDto);
        }

        return parsedDto.Exercises.Count > 0
            ? Result.Success(parsedDto)
            : Result.Failure<ParsedWorkoutDto>("Text was processed, but no valid sets were found.");
    }

    /// <summary>
    ///     Normalizes a weight value to kilograms based on the detected unit.
    ///     If the unit is pounds (lbs/lb), converts to kilograms using the standard conversion factor.
    ///     If the unit is already kilograms (kg), returns the value unchanged.
    /// </summary>
    /// <param name="weight">The weight value to normalize.</param>
    /// <param name="unit">The unit of the weight (e.g., "lbs", "lb", "kg").</param>
    /// <returns>The weight normalized to kilograms with 2 decimal precision.</returns>
    private static decimal NormalizeWeightToKg(decimal weight, string unit)
    {
        var normalizedUnit = unit.ToLowerInvariant().Trim();

        return normalizedUnit switch
        {
            "lbs" or "lb" => Math.Round(weight * LbsToKgConversionFactor, 2),
            "kg" => weight,
            _ => Math.Round(weight * LbsToKgConversionFactor, 2) // Default to lbs for backward compatibility
        };
    }

    /// <summary>
    ///     Regex to capture the header of a workout log, including title and date/time.
    ///     Matches format: "Title\nDayOfWeek, Month Day, Year at Time"
    /// </summary>
    [GeneratedRegex(
        @"^(?<title>.+?)\r?\n(?<dayOfWeek>\w+),\s*(?<month>\w+)\s(?<day>\d{1,2}),\s*(?<year>\d{4})\s*at\s*(?<time>\d{1,2}:\d{2}(?:am|pm))",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex HeaderRegex();

    /// <summary>
    ///     Regex to capture a block of an exercise, including its name and the sets performed.
    ///     Captures the exercise name and all subsequent set lines until the next exercise or end of text.
    /// </summary>
    [GeneratedRegex(@"^(?!.*,.*\d{4}.*at)(?<exerciseName>[^\n\r]+)\r?\n(?<sets>(?:Set\s\d+:.*?(?:\r?\n|$))+)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ExerciseBlockRegex();

    /// <summary>
    ///     Regex to capture the details of a single set, including weight, unit (lbs/lb/kg), reps, and optional RPE.
    ///     Supports formats like "Set 1: 135 lbs x 12" or "Set 2: 60 kg x 10 @ 8 rpe".
    /// </summary>
    [GeneratedRegex(
        @"Set\s(?<setIndex>\d+):\s*(?<weight>[\d\.]+)?\s*(?<unit>lbs|lb|kg)\s*x\s*(?<reps>\d+)(?:\s*@\s*(?<rpe>[\d\.]+)\s*rpe)?",
        RegexOptions.IgnoreCase)]
    private static partial Regex SetRegex();
}