using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;

namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines business logic operations for managing user subscriptions.
/// Handles subscription activation, renewal, cancellation, and payment tracking.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Activates a subscription after successful payment.
    /// Updates UserSubscription (StartDate, EndDate, IsActive = true, StripeSubscriptionId).
    /// Creates a PaymentTransaction record with tax details.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="planId">The unique identifier of the subscription plan.</param>
    /// <param name="stripeSubscriptionId">The Stripe Subscription ID.</param>
    /// <param name="stripeCustomerId">The Stripe Customer ID.</param>
    /// <param name="amount">The total payment amount (including tax).</param>
    /// <param name="taxAmount">The tax amount charged.</param>
    /// <param name="currency">The currency used for the payment.</param>
    /// <param name="countryCode">The user's country code for tax compliance.</param>
    /// <param name="regionCode">The user's region/province/state code.</param>
    /// <returns>The created or updated UserSubscription entity.</returns>
    Task<UserSubscription> ActivateSubscriptionAsync(
        string userId,
        Guid planId,
        string stripeSubscriptionId,
        string stripeCustomerId,
        decimal amount,
        decimal taxAmount,
        Currency currency,
        string countryCode,
        string? regionCode = null);

    /// <summary>
    /// Deactivates a subscription (sets IsActive = false, records CancelledAt timestamp).
    /// </summary>
    /// <param name="stripeSubscriptionId">The Stripe Subscription ID to deactivate.</param>
    /// <param name="cancellationReason">The reason for cancellation.</param>
    /// <returns>True if deactivation was successful; otherwise, false.</returns>
    Task<bool> DeactivateSubscriptionAsync(string stripeSubscriptionId, string? cancellationReason = null);

    /// <summary>
    /// Retrieves the active subscription for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The active UserSubscription, or null if none exists.</returns>
    Task<UserSubscription?> GetActiveSubscriptionAsync(string userId);

    /// <summary>
    /// Retrieves all subscriptions for a user (active and historical).
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list of all user subscriptions.</returns>
    Task<IReadOnlyList<UserSubscription>> GetUserSubscriptionsAsync(string userId);

    /// <summary>
    /// Checks if a user has an active subscription.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>True if the user has an active subscription; otherwise, false.</returns>
    Task<bool> HasActiveSubscriptionAsync(string userId);

    /// <summary>
    /// Records a successful payment transaction.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="amount">The total payment amount.</param>
    /// <param name="taxAmount">The tax amount charged.</param>
    /// <param name="currency">The currency used.</param>
    /// <param name="gatewayTransactionId">The Stripe Payment Intent ID or Charge ID.</param>
    /// <param name="stripeSubscriptionId">The associated Stripe Subscription ID.</param>
    /// <param name="stripeInvoiceId">The Stripe Invoice ID (if applicable).</param>
    /// <param name="countryCode">The user's country code.</param>
    /// <param name="regionCode">The user's region/province/state code.</param>
    /// <returns>The created PaymentTransaction entity.</returns>
    Task<PaymentTransaction> RecordPaymentAsync(
        string userId,
        decimal amount,
        decimal taxAmount,
        Currency currency,
        string gatewayTransactionId,
        string? stripeSubscriptionId = null,
        string? stripeInvoiceId = null,
        string? countryCode = null,
        string? regionCode = null);
}
