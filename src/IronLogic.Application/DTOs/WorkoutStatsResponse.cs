namespace IronLogic.Application.DTOs;

/// <summary>
///     DTO for workout statistics returned by the stats endpoint.
///     Volume = Weight × Reps.
/// </summary>
public class WorkoutStatsResponse
{
    public int TotalSessions { get; set; }

    public int TotalExercises { get; set; }

    public int TotalSets { get; set; }

    /// <summary>
    ///     Sum of (Weight × Reps) across all sets for the most recent session.
    /// </summary>
    public double TotalVolume { get; set; }

    /// <summary>
    ///     The exercise with the highest volume in the most recent session,
    ///     or <c>null</c> if no sessions are available.
    /// </summary>
    public string? TopExercise { get; set; }

    /// <summary>
    ///     Average weight per rep (Total Volume / Total Reps) for the most recent session.
    ///     Indicates how "heavy" the session was.
    /// </summary>
    public double IntensityScore { get; set; }
}