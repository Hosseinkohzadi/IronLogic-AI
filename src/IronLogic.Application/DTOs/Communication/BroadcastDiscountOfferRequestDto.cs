using System.ComponentModel.DataAnnotations;

namespace IronLogic.Application.DTOs.Communication;

/// <summary>
/// Request payload for broadcasting a discount-offer email campaign.
/// </summary>
public record BroadcastDiscountOfferRequestDto
{
    /// <summary>
    /// Gets the subject line used for the discount-offer email.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Subject { get; init; } = "Limited Time Discount Offer";

    /// <summary>
    /// Gets the discount percentage value displayed in email templates.
    /// </summary>
    [Range(0, 100)]
    public decimal DiscountPercentage { get; init; }

    /// <summary>
    /// Gets an optional custom message included in the campaign.
    /// </summary>
    [StringLength(2000)]
    public string? CustomMessage { get; init; }

    /// <summary>
    /// Gets the call-to-action URL.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string CallToActionUrl { get; init; } = "https://app.ironlogic.ai";
}
