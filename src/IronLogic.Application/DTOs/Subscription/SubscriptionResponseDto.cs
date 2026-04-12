namespace IronLogic.Application.DTOs.Subscription;

/// <summary>
/// Data transfer object for subscription response
/// </summary>
/// <param name="Success">Indicates whether the subscription was successful</param>
/// <param name="Message">Status message for the subscription attempt</param>
/// <param name="TransactionId">Unique transaction identifier</param>
/// <param name="SubscriptionId">Unique subscription identifier if successful</param>
public record SubscriptionResponseDto(
    bool Success,
    string Message,
    string TransactionId,
    Guid? SubscriptionId = null);
