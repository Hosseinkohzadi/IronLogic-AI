namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines operations for managing and retrieving exercises with approval workflow support.
/// </summary>
public interface IExerciseService
{
    /// <summary>
    /// Retrieves all exercises available to a specific user.
    /// This includes all approved exercises plus private exercises created by the user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A read-only list of available exercises, including their ImageUrl property.</returns>
    Task<IReadOnlyList<Domain.Entities.Exercise>> GetAvailableExercisesAsync(string userId);

    /// <summary>
    /// Approves an exercise, making it globally visible to all users.
    /// This operation is restricted to ADMIN role and sets Status to Approved and IsGlobal to true.
    /// </summary>
    /// <param name="exerciseId">The unique identifier of the exercise to approve.</param>
    /// <returns>True if the exercise was successfully approved; otherwise, false.</returns>
    Task<bool> ApproveExerciseAsync(Guid exerciseId);

    /// <summary>
    /// Retrieves exercises created by a specific user, including their ImageUrl.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A read-only list of exercises created by the user.</returns>
    Task<IReadOnlyList<Domain.Entities.Exercise>> GetExercisesByCreatorAsync(string userId);

    /// <summary>
    /// Retrieves all exercises pending admin approval.
    /// </summary>
    /// <returns>A read-only list of exercises with status PendingApproval.</returns>
    Task<IReadOnlyList<Domain.Entities.Exercise>> GetPendingApprovalsAsync();
}
