namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a user's active or historical subscription to a specific plan.
/// </summary>
public class UserSubscription : BaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who owns this subscription.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the subscription plan.
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the subscription plan.
    /// </summary>
    public SubscriptionPlan? Plan { get; set; }

    /// <summary>
    /// Gets or sets the start date of the subscription.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the subscription.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets whether the subscription is currently active.
    /// </summary>
    public bool IsActive { get; set; }
}
