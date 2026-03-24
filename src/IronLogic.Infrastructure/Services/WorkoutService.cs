using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;

namespace IronLogic.Infrastructure.Services;

public class WorkoutService(IWorkoutSessionRepository repository) : IWorkoutService
{
    public async Task<List<WorkoutSession>> GetSessionsAsync()
    {
        return await repository.GetAllWithExercisesAndSetsAsync();
    }

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
            TotalVolume = currentMonthSets.Sum(s => (s.Weight ?? 0) * (s.Reps ?? 0)) // Volume = Weight * Reps (current month only)
        };
    }
}