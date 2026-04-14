using IronLogic.Application.DTOs.User;

namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines administrative operations for managing exercises, subscriptions, and system-level tasks.
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Approves an exercise, making it globally visible to all users.
    /// </summary>
    /// <param name="exerciseId">The unique identifier of the exercise to approve.</param>
    /// <returns>True if the exercise was successfully approved; otherwise, false.</returns>
    Task<bool> ApproveExerciseAsync(Guid exerciseId);

    /// <summary>
    /// Rejects an exercise submission.
    /// </summary>
    /// <param name="exerciseId">The unique identifier of the exercise to reject.</param>
    /// <param name="reason">Optional reason for rejection.</param>
    /// <returns>True if the exercise was successfully rejected; otherwise, false.</returns>
    Task<bool> RejectExerciseAsync(Guid exerciseId, string? reason = null);

    /// <summary>
    /// Retrieves administrative metrics for the user management dashboard.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User metrics including premium subscribers, active users, sessions, and churn risk.</returns>
    Task<AdminUserMetricsDto> GetUserMetricsAsync(CancellationToken cancellationToken);
}
