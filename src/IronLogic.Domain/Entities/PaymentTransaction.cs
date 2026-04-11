using IronLogic.Domain.Enums;

namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a financial transaction related to subscription purchases or renewals.
/// Integrates with Stripe for payment processing with full tax calculation support.
/// Transaction precision is decimal(18,2) for accurate financial calculations.
/// </summary>
public class PaymentTransaction : BaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who initiated the transaction.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount in the specified currency.
    /// Precision: decimal(18,2) for accurate financial calculations.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency for this transaction (USD, CAD, EUR, GBP, AUD).
    /// Essential for multi-currency support in global markets.
    /// </summary>
    public Currency Currency { get; set; } = Currency.USD;

    /// <summary>
    /// Gets or sets the tax amount for this transaction in the specified currency.
    /// Handles Canadian GST/HST and international tax requirements.
    /// Precision: decimal(18,2).
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the two-letter ISO country code (e.g., "CA", "US", "GB") for tax calculation.
    /// Essential for Canadian GST/HST (varies by province) and international tax compliance.
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the province/state code for regional tax calculation (e.g., "ON", "BC", "CA", "NY").
    /// Critical for Canadian provinces with different HST/PST/GST rates.
    /// </summary>
    public string? RegionCode { get; set; }

    /// <summary>
    /// Gets or sets the unique transaction identifier from the payment gateway (Stripe Payment Intent ID or Charge ID).
    /// Format: "pi_..." or "ch_..." - Essential for Stripe webhook handling and reconciliation.
    /// </summary>
    public string GatewayTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stripe Subscription ID if this transaction is part of a recurring subscription.
    /// Format: "sub_..." - Links transaction to subscription for billing history.
    /// </summary>
    public string? StripeSubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the Stripe Invoice ID if this transaction is associated with a subscription invoice.
    /// Format: "in_..." - Used for subscription billing and invoice generation.
    /// </summary>
    public string? StripeInvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the status of the transaction (Pending, Completed, Failed, Refunded, Cancelled).
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Gets or sets the payment method used (e.g., "card", "bank_transfer", "paypal").
    /// </summary>
    public string PaymentMethod { get; set; } = "card";

    /// <summary>
    /// Gets or sets the last 4 digits of the card or payment method identifier.
    /// For display purposes only - never store full card numbers.
    /// </summary>
    public string? PaymentMethodLast4 { get; set; }

    /// <summary>
    /// Gets or sets a description for this transaction (e.g., "Monthly Pro Plan Subscription").
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was processed in UTC.
    /// All DateTime fields use UTC for consistent timezone handling.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets the error message if the payment failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the refund amount if this transaction was partially or fully refunded.
    /// Precision: decimal(18,2).
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the refund was processed in UTC.
    /// </summary>
    public DateTime? RefundedAt { get; set; }
}
