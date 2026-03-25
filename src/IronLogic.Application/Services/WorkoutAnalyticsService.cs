using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;

namespace IronLogic.Application.Services;

/// <summary>
///     Provides common analytics operations for workout sessions and exercises.
/// </summary>
public class WorkoutAnalyticsService : IWorkoutAnalyticsService
{
    /// <inheritdoc />
    public double CalculateSessionVolume(HevyWorkoutSessionDto session)
    {
        if (session is null) return 0.0;

        // Sum weight * reps across all exercises and sets, treating null weight/reps as 0
        return session.Exercises?
            .SelectMany(e => e?.Sets ?? Enumerable.Empty<HevySetDto>())
            .Select(s => (s?.Weight ?? 0.0) * (s?.Reps ?? 0))
            .Sum() ?? 0.0;
    }

    /// <inheritdoc />
    public bool IsPersonalRecord(HevyExerciseDto currentExercise, IEnumerable<HevyWorkoutSessionDto> history)
    {
        if (currentExercise is null) return false;

        // Compute the best single-set volume for the current exercise
        var currentMax = currentExercise.Sets?
            .Select(s => (s?.Weight ?? 0.0) * (s?.Reps ?? 0))
            .DefaultIfEmpty(0.0)
            .Max() ?? 0.0;

        // Walk history to find the historical best single-set volume for the same exercise name
        var historicalMax = 0.0;
        if (history != null)
            foreach (var session in history)
            {
                if (session?.Exercises == null) continue;

                foreach (var ex in session.Exercises)
                {
                    if (ex is null) continue;
                    if (!string.Equals(ex.Name, currentExercise.Name, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var exMax = ex.Sets?
                        .Select(s => (s?.Weight ?? 0.0) * (s?.Reps ?? 0))
                        .DefaultIfEmpty(0.0)
                        .Max() ?? 0.0;

                    if (exMax > historicalMax) historicalMax = exMax;
                }
            }

        // If no historical data exists (historicalMax == 0) then any non-zero currentMax is a PR.
        return currentMax > historicalMax;
    }
}