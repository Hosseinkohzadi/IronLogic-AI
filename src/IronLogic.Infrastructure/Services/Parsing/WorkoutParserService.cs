using System.Globalization;
using System.Text.RegularExpressions;
using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Interfaces;
using IronLogic.Application.Shared;

namespace IronLogic.Infrastructure.Services.Parsing;

/// <summary>
///     A service responsible for parsing raw workout text into a structured <see cref="ParsedWorkoutDto" />.
/// </summary>
public partial class WorkoutParserService : IWorkoutParserService
{
    /// <summary>
    ///     Parses a raw string of workout data into a structured <see cref="ParsedWorkoutDto" />.
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

        // Using Value for string manipulations like Trim.
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
                exerciseDto.Sets.Add(new ParsedSetDto
                {
                    // 🚀 Use ValueSpan to avoid string allocation.
                    SetIndex = int.Parse(setMatch.Groups["setIndex"].ValueSpan),

                    Weight = setMatch.Groups["weight"].Success
                        ? decimal.Parse(setMatch.Groups["weight"].ValueSpan, CultureInfo.InvariantCulture)
                        : 0,

                    Reps = int.Parse(setMatch.Groups["reps"].ValueSpan),

                    Rpe = setMatch.Groups["rpe"].Success
                        ? decimal.Parse(setMatch.Groups["rpe"].ValueSpan, CultureInfo.InvariantCulture)
                        : null
                });

            // Using Count > 0 is more performant than Any().
            if (exerciseDto.Sets.Count > 0)
                parsedDto.Exercises.Add(exerciseDto);
        }

        return parsedDto.Exercises.Count > 0
            ? Result.Success(parsedDto)
            : Result.Failure<ParsedWorkoutDto>("Text was processed, but no valid sets were found.");
    }

    /// <summary>
    ///     Regex to capture the header of a workout log, including title and date/time.
    /// </summary>
    [GeneratedRegex(
        @"^(?<title>.+?)\r?\n(?<dayOfWeek>\w+),\s*(?<month>\w+)\s(?<day>\d{1,2}),\s*(?<year>\d{4})\s*at\s*(?<time>\d{1,2}:\d{2}(?:am|pm))",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex HeaderRegex();

    /// <summary>
    ///     Regex to capture a block of an exercise, including its name and the sets performed.
    /// </summary>
    [GeneratedRegex(@"^(?!.*,.*\d{4}.*at)(?<exerciseName>[^\n\r]+)\r?\n(?<sets>(?:Set\s\d+:.*?(?:\r?\n|$))+)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ExerciseBlockRegex();

    /// <summary>
    ///     Regex to capture the details of a single set, including weight, reps, and optional RPE.
    /// </summary>
    [GeneratedRegex(
        @"Set\s(?<setIndex>\d+):\s*(?<weight>[\d\.]+)?\s*lbs\s*x\s*(?<reps>\d+)(?:\s*@\s*(?<rpe>[\d\.]+)\s*rpe)?",
        RegexOptions.IgnoreCase)]
    private static partial Regex SetRegex();
}