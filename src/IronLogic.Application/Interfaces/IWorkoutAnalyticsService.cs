using IronLogic.Application.DTOs;

namespace IronLogic.Application.Interfaces;

/// <summary>
///     Analytics helpers for workout sessions and exercises.
///     Output from these methods feeds into <see cref="ICoachService"/> to generate
///     bodybuilding advice for Classic Physique athletes.
/// </summary>
public interface IWorkoutAnalyticsService
{
    /// <summary>
    ///     Calculates the total session volume as the sum of (weight × reps) across all sets and exercises.
    ///     Returns 0 when the session or numeric values are missing.
    /// </summary>
    double CalculateTotalVolume(HevyWorkoutSessionDto session);

    /// <summary>
    ///     Calculates the total number of reps across all sets and exercises in a session.
    ///     Treats null reps as 0.
    /// </summary>
    int CalculateTotalReps(HevyWorkoutSessionDto session);

    /// <summary>
    ///     Returns volume (weight × reps) broken down by exercise name.
    ///     Treats null weights/reps as 0.
    /// </summary>
    Dictionary<string, double> CalculateVolumePerExercise(HevyWorkoutSessionDto session);

    /// <summary>
    ///     Calculates the average weight per rep (Total Volume / Total Reps) to indicate
    ///     how heavy the session was. Returns 0 when total reps is 0.
    /// </summary>
    double GetIntensityScore(HevyWorkoutSessionDto session);

    /// <summary>
    ///     Returns the exercise with the highest total volume (weight × reps) in the session,
    ///     or <c>null</c> when the session contains no exercises or all volumes are zero.
    /// </summary>
    HevyExerciseDto? GetTopExercise(HevyWorkoutSessionDto session);

    /// <summary>
    ///     Returns true when the provided exercise contains a set whose single-set volume
    ///     (weight × reps) exceeds any historical single-set volume for the same exercise name.
    ///     Handles null weights/reps safely (treated as 0).
    ///     If no history exists for the exercise, a non-zero current set will be considered a PR.
    /// </summary>
    bool IsPersonalRecord(HevyExerciseDto currentExercise, IEnumerable<HevyWorkoutSessionDto> history);
}