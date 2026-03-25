namespace IronLogic.Application.DTOs;

/// <summary>
/// DTO representing a single set performed in an exercise from Hevy.
/// </summary>
public sealed class HevySetDto
{
    public double? Weight { get; set; }

    public int? Reps { get; set; }

    /// <summary>
    /// Type of the set (e.g., "work", "warmup", "drop", etc.).
    /// </summary>
    public string SetType { get; set; } = string.Empty;
}