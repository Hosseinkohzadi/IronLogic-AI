namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a subscription plan offering specific features for a duration and price.
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    /// <summary>
    /// Gets or sets the name of the subscription plan (e.g., "Basic", "Premium", "Pro").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the subscription plan in the base currency.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the duration of the subscription in days.
    /// </summary>
    public int DurationDays { get; set; }

    /// <summary>
    /// Gets or sets a JSON-serialized string containing the features included in this plan.
    /// </summary>
    public string? FeaturesJson { get; set; }

    /// <summary>
    /// Gets or sets the collection of user subscriptions associated with this plan.
    /// </summary>
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
