namespace IronLogic.Application.DTOs.Subscription;

/// <summary>
/// Data transfer object for payment transaction information with user details
/// </summary>
/// <param name="TransactionId">Unique transaction identifier</param>
/// <param name="UserId">User's unique identifier</param>
/// <param name="UserEmail">User's email address</param>
/// <param name="UserName">User's display name</param>
/// <param name="Amount">Transaction amount</param>
/// <param name="Currency">Currency code</param>
/// <param name="Status">Payment status</param>
/// <param name="PaymentMethod">Payment method used</param>
/// <param name="Description">Transaction description</param>
/// <param name="ProcessedAt">When the payment was processed (UTC)</param>
/// <param name="CreatedAt">When the transaction was created (UTC)</param>
public record PaymentTransactionDto(
    Guid TransactionId,
    string UserId,
    string UserEmail,
    string? UserName,
    decimal Amount,
    string Currency,
    string Status,
    string PaymentMethod,
    string? Description,
    DateTime? ProcessedAt,
    DateTime CreatedAt);
