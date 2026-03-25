using IronLogic.Application.DTOs;

namespace IronLogic.Application.Interfaces;

/// <summary>
///     Analytics helpers for workout sessions and exercises.
/// </summary>
public interface IWorkoutAnalyticsService
{
    /// <summary>
    ///     Calculates the session volume as the sum of (weight * reps) across all sets and exercises.
    ///     Returns 0 when the session or numeric values are missing.
    /// </summary>
    double CalculateSessionVolume(HevyWorkoutSessionDto session);

    /// <summary>
    ///     Returns true when the provided exercise contains a set whose single-set volume
    ///     (weight * reps) exceeds any historical single-set volume for the same exercise name.
    ///     Handles null weights/reps safely (treated as 0).
    ///     If no history exists for the exercise, a non-zero current set will be considered a PR.
    /// </summary>
    bool IsPersonalRecord(HevyExerciseDto currentExercise, IEnumerable<HevyWorkoutSessionDto> history);
}