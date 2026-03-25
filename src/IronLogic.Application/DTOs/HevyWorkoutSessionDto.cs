namespace IronLogic.Application.DTOs;

/// <summary>
///     DTO representing a workout session received from the Hevy API.
/// </summary>
public sealed class HevyWorkoutSessionDto
{
    public Guid Id { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<HevyExerciseDto> Exercises { get; set; } = new();
}