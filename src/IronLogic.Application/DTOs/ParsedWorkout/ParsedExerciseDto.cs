namespace IronLogic.Application.DTOs.ParsedWorkout;

/// <summary>
/// Represents a single exercise parsed from the workout log, including its name and all associated sets.
/// </summary>
public class ParsedExerciseDto
{
    /// <summary>
    /// Gets or sets the name of the exercise (e.g., "Incline Bench Press (Smith Machine)").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the list of sets performed for this exercise.
    /// </summary>
    public List<ParsedSetDto> Sets { get; set; } = new();
}