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
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list
    ///     of <see cref="WorkoutSession" /> objects. The list may be empty if no sessions exist.
    /// </returns>
    public async Task<List<WorkoutSession>> GetSessionsAsync()
    {
        return await repository.GetAllWithExercisesAndSetsAsync();
    }

    /// <summary>
    ///     Computes aggregate workout statistics from locally stored sessions.
    ///     Volume is scoped to the current UTC month. The most recent session's date,
    ///     top exercise (by volume), and an intensity score are included when data exists.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is a populated
    ///     <see cref="WorkoutStatsResponse" /> instance with safe defaults when no data is found.
    /// </returns>
    public async Task<WorkoutStatsResponse> GetStatsAsync()
    {
        var allSessions = await repository.GetAllWithExercisesAndSetsAsync();

        // Volume is scoped to the current month
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

        var currentMonthSessions = await repository.GetByDateRangeWithExercisesAndSetsAsync(monthStart, monthEnd);
        var currentMonthSets = currentMonthSessions
            .SelectMany(s => s.Exercises)
            .SelectMany(e => e.Sets)
            .ToList();

        var totalVolume = currentMonthSets.Sum(s => (s.Weight ?? 0) * (s.Reps ?? 0));
        var totalReps = currentMonthSets.Sum(s => s.Reps ?? 0);

        var mostRecentSession = allSessions
            .OrderByDescending(s => s.Date)
            .FirstOrDefault();

        // Top exercise = the one with the highest volume across the current month
        var topExercise = currentMonthSessions
            .SelectMany(s => s.Exercises)
            .Select(e => new
            {
                e.Name,
                Volume = e.Sets.Sum(s => (s.Weight ?? 0) * (s.Reps ?? 0))
            })
            .OrderByDescending(e => e.Volume)
            .FirstOrDefault();

        return new WorkoutStatsResponse
        {
            TotalVolume = totalVolume,
            TopExercise = topExercise?.Name,
            IntensityScore = totalReps > 0 ? totalVolume / totalReps : 0.0,
            SessionDate = mostRecentSession?.Date
        };
    }
}