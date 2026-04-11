using IronLogic.Domain.Entities;

namespace IronLogic.Domain.Interfaces;

/// <summary>
/// Defines repository operations specific to Exercise entities with approval workflow support.
/// </summary>
public interface IExerciseRepository : IGenericRepository<Exercise>
{
    /// <summary>
    /// Retrieves all exercises available to a specific user.
    /// This includes all globally approved exercises plus private exercises created by the user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A read-only list of available exercises.</returns>
    Task<IReadOnlyList<Exercise>> GetAvailableExercisesAsync(string userId);

    /// <summary>
    /// Retrieves all exercises pending admin approval.
    /// </summary>
    /// <returns>A read-only list of exercises with status PendingApproval.</returns>
    Task<IReadOnlyList<Exercise>> GetPendingApprovalsAsync();

    /// <summary>
    /// Retrieves exercises created by a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A read-only list of exercises created by the user.</returns>
    Task<IReadOnlyList<Exercise>> GetExercisesByCreatorAsync(string userId);
}
