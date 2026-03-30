namespace IronLogic.Application.DTOs.ParsedWorkout;

/// <summary>
/// Represents the structured data parsed from a raw workout text log.
/// This is an intermediate representation before creating domain entities.
/// </summary>
public class ParsedWorkoutDto
{
    /// <summary>
    /// Gets or sets the title of the workout session (e.g., "Evening workout 🏋️").
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the date and time the workout session occurred.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the list of exercises parsed from the workout log.
    /// </summary>
    public List<ParsedExerciseDto> Exercises { get; set; } = new();
}