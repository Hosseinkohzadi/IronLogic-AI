namespace IronLogic.Application.DTOs.Settings;

/// <summary>
/// Public pricing configuration returned to client applications.
/// </summary>
public record PricingConfigDto
{
    /// <summary>
    /// Gets the yearly subscription discount percentage.
    /// </summary>
    public decimal YearlyDiscountPercentage { get; init; }
}
