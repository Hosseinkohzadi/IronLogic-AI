using IronLogic.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Repositories;

/// <summary>
///     EF Core implementation of <see cref="IWorkoutSessionRepository" />.
///     Encapsulates all WorkoutSession data access including eager loading of the full aggregate.
/// </summary>
public class WorkoutSessionRepository(AppDbContext dbContext) : IWorkoutSessionRepository
{
    public async Task<IEnumerable<WorkoutSession>> GetAllAsync()
    {
        return await dbContext.Sessions
            .Include(s => s.Exercises)
            .ThenInclude(e => e.Sets)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<WorkoutSession?> GetByIdAsync(Guid id)
    {
        return await dbContext.Sessions
            .Include(s => s.Exercises)
            .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(WorkoutSession session)
    {
        await dbContext.Sessions.AddAsync(session);
        await dbContext.SaveChangesAsync();
    }

    public async Task<float> GetTotalVolumeAsync(int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        var totalVolume = await dbContext.Sessions
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .SelectMany(s => s.Exercises)
            .SelectMany(e => e.Sets)
            .SumAsync(s => (float)((s.Weight ?? 0) * (s.Reps ?? 0)));

        return totalVolume;
    }

    public async Task<List<WorkoutSession>> GetAllWithExercisesAndSetsAsync()
    {
        return await dbContext.Sessions
            .Include(s => s.Exercises)
            .ThenInclude(e => e.Sets)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<List<WorkoutSession>> GetByDateRangeWithExercisesAndSetsAsync(DateTime startDate,
        DateTime endDate)
    {
        return await dbContext.Sessions
            .Include(s => s.Exercises)
            .ThenInclude(e => e.Sets)
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }
}