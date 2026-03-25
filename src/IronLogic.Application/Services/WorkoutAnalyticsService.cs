using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;

namespace IronLogic.Application.Services;

/// <summary>
///     Provides common analytics operations for workout sessions and exercises.
///     Results are consumed by <see cref="CoachService" /> to generate tailored
///     Classic Physique coaching advice.
/// </summary>
public class WorkoutAnalyticsService : IWorkoutAnalyticsService
{
    /// <inheritdoc />
    public double CalculateTotalVolume(HevyWorkoutSessionDto session)
    {
        if (session is null) return 0.0;

        return session.Exercises?
            .SelectMany(e => e?.Sets ?? Enumerable.Empty<HevySetDto>())
            .Select(s => (s?.Weight ?? 0.0) * (s?.Reps ?? 0))
            .Sum() ?? 0.0;
    }

    /// <inheritdoc />
    public int CalculateTotalReps(HevyWorkoutSessionDto session)
    {
        if (session is null) return 0;

        return session.Exercises?
            .SelectMany(e => e?.Sets ?? Enumerable.Empty<HevySetDto>())
            .Sum(s => s?.Reps ?? 0) ?? 0;
    }

    /// <inheritdoc />
    public Dictionary<string, double> CalculateVolumePerExercise(HevyWorkoutSessionDto session)
    {
        if (session?.Exercises is null)
            return new Dictionary<string, double>();

        return session.Exercises
            .Where(e => e is not null)
            .GroupBy(e => e.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(e => e.Sets ?? Enumerable.Empty<HevySetDto>())
                    .Sum(s => (s?.Weight ?? 0.0) * (s?.Reps ?? 0)),
                StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public double GetIntensityScore(HevyWorkoutSessionDto session)
    {
        var totalVolume = CalculateTotalVolume(session);
        var totalReps = CalculateTotalReps(session);

        return totalReps > 0 ? totalVolume / totalReps : 0.0;
    }

    /// <inheritdoc />
    public HevyExerciseDto? GetTopExercise(HevyWorkoutSessionDto session)
    {
        if (session?.Exercises is null or { Count: 0 })
            return null;

        HevyExerciseDto? top = null;
        var topVolume = 0.0;

        foreach (var exercise in session.Exercises)
        {
            if (exercise is null) continue;

            var volume = exercise.Sets?
                .Select(s => (s?.Weight ?? 0.0) * (s?.Reps ?? 0))
                .Sum() ?? 0.0;

            if (!(volume > topVolume))
                continue;

            topVolume = volume;
            top = exercise;
        }

        return top;
    }

    /// <inheritdoc />
    public bool IsPersonalRecord(HevyExerciseDto currentExercise, IEnumerable<HevyWorkoutSessionDto> history)
    {
        if (currentExercise is null) return false;

        var currentMax = currentExercise.Sets?
            .Select(s => (s?.Weight ?? 0.0) * (s?.Reps ?? 0))
            .DefaultIfEmpty(0.0)
            .Max() ?? 0.0;

        var historicalMax = 0.0;
        if (history == null)
            return currentMax > historicalMax;
        {
            foreach (var session in history)
            {
                if (session?.Exercises == null) continue;

                historicalMax = (from ex in session.Exercises.OfType<HevyExerciseDto>()
                        where string.Equals(ex.Name, currentExercise.Name, StringComparison.OrdinalIgnoreCase)
                        select ex.Sets?.Select(s => (s?.Weight ?? 0.0) * (s?.Reps ?? 0))
                            .DefaultIfEmpty(0.0)
                            .Max() ?? 0.0).Prepend(historicalMax)
                    .Max();
            }
        }

        return currentMax > historicalMax;
    }
}