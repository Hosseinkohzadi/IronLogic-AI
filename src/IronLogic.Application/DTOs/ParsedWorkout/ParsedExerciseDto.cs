using IronLogic.Application.DTOs.ParsedWorkout;

/// <summary>
/// Represents a single exercise parsed from a raw workout log.
/// </summary>
public class ParsedExerciseDto
{
    /// <summary>
    /// Gets or sets the name of the exercise.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of sets performed for this exercise.
    /// </summary>
    public List<ParsedSetDto> Sets { get; set; } = new();

    /// <summary>
    /// Gets or sets the personal record (PR) insight for this exercise, if any.
    /// This is populated after analyzing the user's history.
    /// </summary>
    public PrInsightDto? PrInsight { get; set; }
}

/// <summary>
/// Represents an insight into a personal record (PR) achieved for an exercise.
/// </summary>
/// <param name="IsNewRecord">Indicates whether a new PR was set.</param>
/// <param name="CurrentMaxWeight">The maximum weight lifted in the current session for this exercise.</param>
/// <param name="PreviousMaxWeight">The previous maximum weight lifted for this exercise, if available.</param>
/// <param name="PreviousDate">The date the previous maximum weight was lifted, if available.</param>
public record PrInsightDto(
    bool IsNewRecord,
    decimal CurrentMaxWeight,
    decimal? PreviousMaxWeight,
    DateTime? PreviousDate
);