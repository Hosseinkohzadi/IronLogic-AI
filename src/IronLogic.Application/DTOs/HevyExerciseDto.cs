namespace IronLogic.Application.DTOs;

/// <summary>
///     DTO representing an exercise inside a Hevy workout session.
/// </summary>
public sealed class HevyExerciseDto
{
    public string Name { get; set; } = string.Empty;

    public List<HevySetDto> Sets { get; set; } = [];
}