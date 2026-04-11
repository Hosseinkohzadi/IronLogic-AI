namespace IronLogic.Domain.Enums;

/// <summary>
/// Defines the visibility and approval status of an exercise.
/// </summary>
public enum ExerciseStatus
{
    /// <summary>
    /// Exercise is visible only to the creator user.
    /// </summary>
    Private = 0,

    /// <summary>
    /// Exercise has been submitted for admin review.
    /// </summary>
    PendingApproval = 1,

    /// <summary>
    /// Exercise has been approved by admin and is globally visible.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Exercise was rejected during admin review.
    /// </summary>
    Rejected = 3
}
