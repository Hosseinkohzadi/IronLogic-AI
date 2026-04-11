using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;
using IronLogic.Domain.Interfaces;

namespace IronLogic.Infrastructure.Repositories;

/// <summary>
/// Provides implementation of exercise-specific repository operations including approval workflow.
/// </summary>
/// <param name="context">The database context.</param>
public class ExerciseRepository(AppDbContext context) : GenericRepository<Exercise>(context), IExerciseRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Exercise>> GetAvailableExercisesAsync(string userId)
    {
        return await _context.Exercises
            .Where(e => e.IsGlobal || e.CreatorUserId == userId)
            .Include(e => e.PrimaryMuscle)
            .Include(e => e.Equipment)
            .Include(e => e.SecondaryMuscles)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Exercise>> GetPendingApprovalsAsync()
    {
        return await _context.Exercises
            .Where(e => e.Status == ExerciseStatus.PendingApproval)
            .Include(e => e.CreatorUser)
            .Include(e => e.PrimaryMuscle)
            .Include(e => e.Equipment)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Exercise>> GetExercisesByCreatorAsync(string userId)
    {
        return await _context.Exercises
            .Where(e => e.CreatorUserId == userId)
            .Include(e => e.PrimaryMuscle)
            .Include(e => e.Equipment)
            .ToListAsync();
    }
}
