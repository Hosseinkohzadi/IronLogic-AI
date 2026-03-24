namespace IronLogic.Application.DTOs;

/// <summary>
/// DTO for workout statistics. Volume = Weight * Reps, scoped to the current month.
/// </summary>
public class WorkoutStatsResponse
{
    public int TotalSessions { get; set; }

    public int TotalExercises { get; set; }

    public int TotalSets { get; set; }

    /// <summary>
    /// Sum of (Weight * Reps) across all sets for the current calendar month.
    /// </summary>
    public double TotalVolume { get; set; }
}