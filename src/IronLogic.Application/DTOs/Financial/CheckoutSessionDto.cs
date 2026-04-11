namespace IronLogic.Application.DTOs.Financial;

/// <summary>
/// DTO for creating a Stripe checkout session request.
/// </summary>
public class CreateCheckoutSessionRequest
{
    /// <summary>
    /// Gets or sets the subscription plan ID.
    /// </summary>
    public Guid planId { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string userId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string userEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the currency for the checkout session (USD, CAD, EUR, GBP, AUD).
    /// </summary>
    public string currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the two-letter ISO country code (e.g., "CA", "US", "GB").
    /// </summary>
    public string countryCode { get; set; } = "US";

    /// <summary>
    /// Gets or sets the province/state code for tax calculation (e.g., "ON", "BC", "CA").
    /// </summary>
    public string? regionCode { get; set; }
}

/// <summary>
/// DTO for Stripe checkout session response.
/// </summary>
public class CheckoutSessionResponse
{
    /// <summary>
    /// Gets or sets the Stripe Checkout Session ID for redirecting to Stripe Checkout.
    /// </summary>
    public string sessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the public URL to redirect the user to Stripe Checkout.
    /// </summary>
    public string checkoutUrl { get; set; } = string.Empty;
}
