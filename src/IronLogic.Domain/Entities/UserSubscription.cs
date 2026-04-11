namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a user's active or historical subscription to a specific plan.
/// Integrates with Stripe for payment processing and subscription management.
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
    /// Gets or sets the start date of the subscription in UTC.
    /// All DateTime fields use UTC for consistent timezone handling across global users.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the subscription in UTC.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets whether the subscription is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether auto-renewal is enabled for this subscription.
    /// </summary>
    public bool AutoRenew { get; set; } = true;

    /// <summary>
    /// Gets or sets the Stripe Subscription ID for payment gateway integration.
    /// Format: "sub_..." - Essential for Stripe webhook handling and subscription management.
    /// </summary>
    public string? StripeSubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the Stripe Customer ID associated with this user.
    /// Format: "cus_..." - Used for managing customer payment methods and billing history.
    /// </summary>
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the cancellation date in UTC if the subscription was cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Gets or sets the reason for cancellation (user-provided or system-generated).
    /// </summary>
    public string? CancellationReason { get; set; }
}
