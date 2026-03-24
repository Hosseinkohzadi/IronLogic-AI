namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a single daily bodyweight log entry for tracking physique progress.
/// </summary>
public class DailyWeight
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime Date { get; set; }

    /// <summary>
    /// Bodyweight in kilograms (kg).
    /// </summary>
    public float Weight { get; set; }

    /// <summary>
    /// Optional note (e.g., "post-refeed", "morning fasted").
    /// </summary>
    public string? Note { get; set; }
}