using System.ComponentModel.DataAnnotations;

namespace IronLogic.Application.DTOs.Subscription;

/// <summary>
/// Data transfer object for creating a new subscription plan
/// </summary>
public record CreatePlanDto
{
    /// <summary>
    /// Gets or sets the name of the plan
    /// </summary>
    [Required(ErrorMessage = "Plan name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Plan name must be between 2 and 100 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the plan
    /// </summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0, 999999.99, ErrorMessage = "Price must be between 0 and 999999.99")]
    public decimal Price { get; init; }

    /// <summary>
    /// Gets or sets the currency code (USD, CAD, EUR, GBP, AUD)
    /// </summary>
    [Required(ErrorMessage = "Currency is required")]
    [RegularExpression("^(USD|CAD|EUR|GBP|AUD)$", ErrorMessage = "Currency must be USD, CAD, EUR, GBP, or AUD")]
    public string Currency { get; init; } = "USD";

    /// <summary>
    /// Gets or sets the subscription duration in days
    /// </summary>
    [Required(ErrorMessage = "Duration is required")]
    [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days")]
    public int DurationDays { get; init; } = 30;

    /// <summary>
    /// Gets or sets the description of the plan
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the list of features included in this plan
    /// </summary>
    public List<string> Features { get; init; } = new();
}
