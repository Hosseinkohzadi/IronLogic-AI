namespace IronLogic.Application.DTOs.Subscription;

/// <summary>
/// Data transfer object for subscription request
/// </summary>
/// <param name="PlanId">The unique identifier of the subscription plan</param>
/// <param name="PaymentMethodId">The payment method identifier from payment gateway</param>
public record SubscribeRequestDto(Guid PlanId, string PaymentMethodId);
