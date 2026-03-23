namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a single flat record (one row) directly parsed from the Hevy CSV export.
/// This acts as a Data Transfer Object (DTO) before mapping to the hierarchical domain entities.
/// </summary>
public class ExerciseRecord
{
    // Mapped from: "start_time"
    public DateTime? Date { get; set; }

    // Mapped from: "title"
    public string WorkoutName { get; set; } = string.Empty;

    // Mapped from: "exercise_title"
    public string ExerciseName { get; set; } = string.Empty;

    // Mapped from: "set_index"
    public int SetOrder { get; set; }

    // Mapped from: "weight_lbs" (or weight_kg)
    public double? Weight { get; set; }

    // Mapped from: "reps"
    public int? Reps { get; set; }

    // Mapped from: "rpe"
    public double? RPE { get; set; }
}