using IronLogic.Application.Interfaces;
using IronLogic.Domain.Enums;
using IronLogic.Domain.Interfaces;

namespace IronLogic.Application.Services;

/// <summary>
/// Implements operations for managing and retrieving exercises with approval workflow support.
/// Handles exercise availability based on approval status and user ownership.
/// </summary>
/// <param name="exerciseRepository">The exercise repository.</param>
public class ExerciseService(IExerciseRepository exerciseRepository) : IExerciseService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Domain.Entities.Exercise>> GetAvailableExercisesAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return await exerciseRepository.GetAvailableExercisesAsync(userId);
    }

    /// <inheritdoc />
    public async Task<bool> ApproveExerciseAsync(Guid exerciseId)
    {
        var exercise = await exerciseRepository.GetByIdAsync(exerciseId);
        
        if (exercise == null)
            return false;

        exercise.Status = ExerciseStatus.Approved;
        exercise.IsGlobal = true;
        
        exerciseRepository.Update(exercise);
        return await exerciseRepository.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Domain.Entities.Exercise>> GetExercisesByCreatorAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return await exerciseRepository.GetExercisesByCreatorAsync(userId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Domain.Entities.Exercise>> GetPendingApprovalsAsync()
    {
        return await exerciseRepository.GetPendingApprovalsAsync();
    }
}
