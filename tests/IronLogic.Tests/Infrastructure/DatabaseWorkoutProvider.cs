using IronLogic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IronLogic.Tests.Infrastructure;

/// <summary>
///     Test-only <see cref="IWorkoutProvider" /> that reads from the InMemory database
///     instead of returning hardcoded mock data. This ensures integration tests that seed
///     the database get consistent results from the /stats endpoint.
/// </summary>
public class DatabaseWorkoutProvider(AppDbContext dbContext) : IWorkoutProvider
{
    public async Task<IEnumerable<HevyWorkoutSessionDto>> GetRecentSessionsAsync(int limit = 10)
    {
        var sessions = await dbContext.Sessions
            .Include(s => s.Exercises)
            .ThenInclude(e => e.Sets)
            .OrderByDescending(s => s.Date)
            .Take(limit)
            .ToListAsync();

        return sessions.Select(s => new HevyWorkoutSessionDto
        {
            Id = s.Id,
            StartTime = s.Date,
            EndTime = s.Date,
            Title = s.Name,
            Exercises = s.Exercises.Select(e => new HevyExerciseDto
            {
                Name = e.Name,
                Sets = e.Sets.Select(set => new HevySetDto
                {
                    Weight = set.Weight,
                    Reps = set.Reps,
                    SetType = "work"
                }).ToList()
            }).ToList()
        });
    }
}