namespace IronLogic.Application.DTOs;

/// <summary>
///     Represents historical data for a specific exercise on a given date.
/// </summary>
/// <param name="Date">The date the exercise was performed.</param>
/// <param name="MaxWeight">The maximum weight lifted for the exercise on this date.</param>
/// <param name="TotalVolume">The total volume (Weight * Reps) for the exercise on this date.</param>
/// <param name="TopSetSummary">A summary of the best set performed for the exercise on this date.</param>
/// <param name="Estimated1RM">The estimated one-repetition maximum for the exercise on this date.</param>
public record ExerciseHistoryPointDto(
    DateTime Date,
    decimal? MaxWeight,
    decimal? TotalVolume,
    string TopSetSummary,
    decimal Estimated1RM
);