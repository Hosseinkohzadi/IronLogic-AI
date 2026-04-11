using IronLogic.Domain.Enums;

namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a subscription plan offering specific features for a duration and price.
/// Supports multi-currency pricing for international markets (CAD, USD, EUR, GBP, AUD).
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    /// <summary>
    /// Gets or sets the name of the subscription plan (e.g., "Basic", "Premium", "Pro").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the subscription plan in the specified currency.
    /// Precision: decimal(18,2) for accurate financial calculations.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the currency for the price (USD, CAD, EUR, GBP, AUD).
    /// Essential for multi-currency support in global markets (Canada, USA, Europe, Australia).
    /// </summary>
    public Currency Currency { get; set; } = Currency.USD;

    /// <summary>
    /// Gets or sets the duration of the subscription in days.
    /// </summary>
    public int DurationDays { get; set; }

    /// <summary>
    /// Gets or sets a JSON-serialized string containing the features included in this plan.
    /// </summary>
    public string? FeaturesJson { get; set; }

    /// <summary>
    /// Gets or sets whether this plan is currently active and available for purchase.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of user subscriptions associated with this plan.
    /// </summary>
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
