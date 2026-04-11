using IronLogic.Domain.Enums;

namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines operations for Stripe payment gateway integration.
/// Handles multi-currency subscriptions (CAD, USD, EUR) and Canadian tax compliance.
/// </summary>
public interface IStripeService
{
    /// <summary>
    /// Creates a Stripe Checkout Session for subscription purchase.
    /// Automatically applies Canadian GST/HST (13% for Ontario) based on user's CountryCode and RegionCode.
    /// </summary>
    /// <param name="planId">The unique identifier of the subscription plan.</param>
    /// <param name="userEmail">The user's email address for the Stripe customer.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="currency">The currency for the checkout session (USD, CAD, EUR, GBP, AUD).</param>
    /// <param name="countryCode">The two-letter ISO country code (e.g., "CA", "US", "GB").</param>
    /// <param name="regionCode">The province/state code for tax calculation (e.g., "ON", "BC", "CA").</param>
    /// <returns>The Checkout Session ID for redirecting the user to Stripe Checkout.</returns>
    Task<string> CreateCheckoutSessionAsync(
        Guid planId,
        string userEmail,
        string userId,
        Currency currency,
        string countryCode,
        string? regionCode = null);

    /// <summary>
    /// Handles incoming Stripe webhook events to activate/deactivate subscriptions.
    /// Processes: invoice.paid, checkout.session.completed, customer.subscription.deleted.
    /// </summary>
    /// <param name="json">The raw JSON payload from Stripe webhook.</param>
    /// <param name="stripeSignature">The Stripe-Signature header value for webhook verification.</param>
    /// <returns>True if the webhook was processed successfully; otherwise, false.</returns>
    Task<bool> HandleWebhookAsync(string json, string stripeSignature);

    /// <summary>
    /// Retrieves the Stripe Customer ID for a user, or creates a new customer if none exists.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="userEmail">The user's email address.</param>
    /// <returns>The Stripe Customer ID (format: "cus_...").</returns>
    Task<string> GetOrCreateCustomerAsync(string userId, string userEmail);

    /// <summary>
    /// Cancels a Stripe subscription at the end of the billing period.
    /// </summary>
    /// <param name="stripeSubscriptionId">The Stripe Subscription ID (format: "sub_...").</param>
    /// <returns>True if cancellation was successful; otherwise, false.</returns>
    Task<bool> CancelSubscriptionAsync(string stripeSubscriptionId);

    /// <summary>
    /// Calculates the tax amount for a given subtotal based on country and region.
    /// Handles Canadian GST/HST rates by province.
    /// </summary>
    /// <param name="subtotal">The subtotal amount before tax.</param>
    /// <param name="countryCode">The two-letter ISO country code.</param>
    /// <param name="regionCode">The province/state code.</param>
    /// <returns>The calculated tax amount.</returns>
    decimal CalculateTaxAmount(decimal subtotal, string countryCode, string? regionCode);
}
