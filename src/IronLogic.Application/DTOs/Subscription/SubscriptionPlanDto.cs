namespace IronLogic.Application.DTOs.Subscription;

/// <summary>
/// Data transfer object for subscription plan information
/// </summary>
/// <param name="Id">Unique identifier for the subscription plan</param>
/// <param name="Name">Name of the plan (e.g., Basic, Pro, Elite)</param>
/// <param name="Price">Monthly price in the specified currency</param>
/// <param name="Currency">Currency code (USD, CAD, EUR, etc.)</param>
/// <param name="Description">Description of the plan</param>
/// <param name="Features">List of features included in this plan</param>
public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    string Description,
    List<string> Features);
