using IronLogic.Application.Interfaces;
using IronLogic.Domain.Enums;
using IronLogic.Domain.Interfaces;

namespace IronLogic.Application.Services;

/// <summary>
/// Implements administrative operations for managing exercises and approvals.
/// </summary>
/// <param name="exerciseRepository">The exercise repository.</param>
public class AdminService(IExerciseRepository exerciseRepository) : IAdminService
{
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
    public async Task<bool> RejectExerciseAsync(Guid exerciseId, string? reason = null)
    {
        var exercise = await exerciseRepository.GetByIdAsync(exerciseId);
        
        if (exercise == null)
            return false;

        exercise.Status = ExerciseStatus.Rejected;
        exercise.IsGlobal = false;
        
        exerciseRepository.Update(exercise);
        return await exerciseRepository.SaveChangesAsync();
    }
}
