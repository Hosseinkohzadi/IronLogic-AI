namespace IronLogic.Domain.Entities;

/// <summary>
///     Represents a single flat record (one row) directly parsed from the Hevy CSV export.
///     This acts as a Data Transfer Object (DTO) before mapping to the hierarchical domain entities.
/// </summary>
public class ExerciseRecord
{
    // The exact date and time the workout started
    public DateTime Date { get; set; }

    // Name of the workout routine (e.g., "Push Day", "Legs")
    public string WorkoutName { get; set; } = string.Empty;

    // Name of the specific exercise (e.g., "Bench Press (Barbell)")
    public string ExerciseName { get; set; } = string.Empty;

    // The sequence number of the set (1, 2, 3...)
    public int SetOrder { get; set; }

    // The weight lifted in this specific set
    public double Weight { get; set; }

    // The number of repetitions completed
    public int Reps { get; set; }

    // Rate of Perceived Exertion - nullable because you might not log it every time
    public double? RPE { get; set; }
}