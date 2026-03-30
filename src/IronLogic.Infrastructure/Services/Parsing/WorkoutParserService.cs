using System.Globalization;
using System.Text.RegularExpressions;
using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Interfaces;
using IronLogic.Application.Shared;

namespace IronLogic.Infrastructure.Services.Parsing;

public partial class WorkoutParserService : IWorkoutParserService
{
    // 1. Header Regex: Sensitive to emoji and exact date format (flexible new line handling)
    [GeneratedRegex(@"^(?<title>.+?)\r?\n(?<dayOfWeek>\w+),\s*(?<month>\w+)\s(?<day>\d{1,2}),\s*(?<year>\d{4})\s*at\s*(?<time>\d{1,2}:\d{2}(?:am|pm))", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex HeaderRegex();

    // 2. Exercise Block Regex: Find the exercise name and all its subsequent sets
    [GeneratedRegex(@"^(?!.*,.*\d{4}.*at)(?<exerciseName>[^\n\r]+)\r?\n(?<sets>(?:Set\s\d+:.*?(?:\r?\n|$))+)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ExerciseBlockRegex();

    // 3. Sets Regex: Extract weight, reps, and RPE (decimal)
    [GeneratedRegex(@"Set\s(?<setIndex>\d+):\s*(?<weight>[\d\.]+)?\s*lbs\s*x\s*(?<reps>\d+)(?:\s*@\s*(?<rpe>[\d\.]+)\s*rpe)?", RegexOptions.IgnoreCase)]
    private static partial Regex SetRegex();

    public Result<ParsedWorkoutDto> Parse(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return Result.Failure<ParsedWorkoutDto>("Raw workout text is empty.");

        // A) Parse Header (Title and Date)
        var headerMatch = HeaderRegex().Match(rawText);
        if (!headerMatch.Success)
            return Result.Failure<ParsedWorkoutDto>("Invalid workout header format. Check the title and date.");

        var parsedDto = new ParsedWorkoutDto
        {
            Title = headerMatch.Groups["title"].Value.Trim()
        };

        // Combine date parts for safe parsing
        var dateString =
            $"{headerMatch.Groups["month"].Value} {headerMatch.Groups["day"].Value} {headerMatch.Groups["year"].Value} {headerMatch.Groups["time"].Value}";
        if (!DateTime.TryParseExact(dateString, "MMM d yyyy h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var parsedDate))
            return Result.Failure<ParsedWorkoutDto>("Workout date or time is not readable.");

        parsedDto.Date = parsedDate;

        // B) Parse Body (Exercises and Sets)
        var exerciseMatches = ExerciseBlockRegex().Matches(rawText);
        if (exerciseMatches.Count == 0)
            return Result.Failure<ParsedWorkoutDto>("No exercises found in the text.");

        foreach (Match exerciseMatch in exerciseMatches)
        {
            var exerciseName = exerciseMatch.Groups["exerciseName"].Value.Trim();

            // Prevent capturing empty lines as an exercise name
            if (string.IsNullOrWhiteSpace(exerciseName))
                continue;

            var exerciseDto = new ParsedExerciseDto
            {
                Name = exerciseName
            };

            var setsText = exerciseMatch.Groups["sets"].Value;
            var setMatches = SetRegex().Matches(setsText);

            foreach (Match setMatch in setMatches)
                exerciseDto.Sets.Add(new ParsedSetDto
                {
                    SetIndex = int.Parse(setMatch.Groups["setIndex"].Value),
                    Weight = setMatch.Groups["weight"].Success
                        ? decimal.Parse(setMatch.Groups["weight"].Value, CultureInfo.InvariantCulture)
                        : 0,
                    Reps = int.Parse(setMatch.Groups["reps"].Value),
                    Rpe = setMatch.Groups["rpe"].Success
                        ? decimal.Parse(setMatch.Groups["rpe"].Value, CultureInfo.InvariantCulture)
                        : null
                });

            if (exerciseDto.Sets.Any())
                parsedDto.Exercises.Add(exerciseDto);
        }

        return parsedDto.Exercises.Any()
            ? Result.Success(parsedDto)
            : Result.Failure<ParsedWorkoutDto>("Text was processed, but no valid sets were found.");
    }
}