namespace IronLogic.Domain.Settings;

/// <summary>
/// Configuration settings for Stripe payment gateway integration.
/// Supports multi-currency (CAD, USD, EUR) and international tax compliance.
/// </summary>
public class StripeSettings
{
    /// <summary>
    /// Gets or sets the Stripe API secret key for server-side operations.
    /// Format: "sk_test_..." (test) or "sk_live_..." (production).
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stripe publishable key for client-side operations.
    /// Format: "pk_test_..." (test) or "pk_live_..." (production).
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the webhook signing secret for verifying webhook authenticity.
    /// Format: "whsec_...".
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the success URL for redirect after successful checkout.
    /// </summary>
    public string SuccessUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cancel URL for redirect if checkout is cancelled.
    /// </summary>
    public string CancelUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to use Stripe Tax for automatic tax calculation.
    /// Recommended for Canadian GST/HST and international tax compliance.
    /// </summary>
    public bool UseStripeTax { get; set; } = true;

    /// <summary>
    /// Gets or sets the default tax rate for Canadian Ontario (HST 13%).
    /// Used when Stripe Tax is disabled or as fallback.
    /// </summary>
    public decimal DefaultCanadianTaxRate { get; set; } = 0.13m;
}
