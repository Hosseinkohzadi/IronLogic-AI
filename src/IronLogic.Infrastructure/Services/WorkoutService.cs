using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Domain.Interfaces;

namespace IronLogic.Infrastructure.Services;

/// <summary>
///     Provides workout-related operations such as retrieving stored workout sessions
///     and computing aggregate statistics (including current-month volume).
/// </summary>
/// <param name="repository">The repository used to read workout sessions, exercises and sets.</param>
public class WorkoutService(IWorkoutSessionRepository repository) : IWorkoutService
{
    /// <summary>
    ///     Retrieves all workout sessions including their exercises and sets.
    /// </summary>
    /// <remarks>
    ///     This method delegates to the injected <paramref name="repository" /> and returns
    ///     hydrated <see cref="WorkoutSession" /> instances with their nested exercises and sets.
    ///     Any exception thrown by the repository (for example, database access errors) will
    ///     propagate to the caller.
    /// </remarks>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list
    ///     of <see cref="WorkoutSession" /> objects. The list may be empty if no sessions exist.
    /// </returns>
    public async Task<List<WorkoutSession>> GetSessionsAsync()
    {
        return await repository.GetAllWithExercisesAndSetsAsync();
    }

    /// <summary>
    ///     Computes aggregate workout statistics for all time and the total training volume for the current month.
    /// </summary>
    /// <remarks>
    ///     The returned <see cref="WorkoutStatsResponse" /> contains:
    ///     - <see cref="WorkoutStatsResponse.TotalSessions" /> : total number of sessions across all time.
    ///     - <see cref="WorkoutStatsResponse.TotalExercises" /> : total number of exercises across all sessions.
    ///     - <see cref="WorkoutStatsResponse.TotalSets" /> : total number of sets across all exercises.
    ///     - <see cref="WorkoutStatsResponse.TotalVolume" /> : summed volume for the current UTC month when volume for a set
    ///     is calculated as (<c>Weight</c> ?? 0) * (<c>Reps</c> ?? 0).
    ///     The current month is computed using UTC (DateTime.UtcNow). This method queries the repository twice:
    ///     1) to obtain all sessions for global totals and 2) to obtain sessions in the current month for volume calculation.
    ///     Any exceptions from repository calls will propagate to the caller.
    /// </remarks>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is a populated
    ///     <see cref="WorkoutStatsResponse" /> instance.
    /// </returns>
    public async Task<WorkoutStatsResponse> GetStatsAsync()
    {
        var allSessions = await repository.GetAllWithExercisesAndSetsAsync();

        var allExercises = allSessions.SelectMany(s => s.Exercises).ToList();
        var allSets = allExercises.SelectMany(e => e.Sets).ToList();

        // Volume is scoped to the current month
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

        var currentMonthSessions = await repository.GetByDateRangeWithExercisesAndSetsAsync(monthStart, monthEnd);
        var currentMonthSets = currentMonthSessions
            .SelectMany(s => s.Exercises)
            .SelectMany(e => e.Sets)
            .ToList();

        return new WorkoutStatsResponse
        {
            TotalSessions = allSessions.Count,
            TotalExercises = allExercises.Count,
            TotalSets = allSets.Count,
            TotalVolume =
                currentMonthSets.Sum(s =>
                    (s.Weight ?? 0) * (s.Reps ?? 0)) // Volume = Weight * Reps (current month only)
        };
    }
}