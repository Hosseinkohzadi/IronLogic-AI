namespace IronLogic.Domain.Enums;

/// <summary>
/// Represents the current status of a payment transaction.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is awaiting processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment has been successfully completed.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Payment failed due to insufficient funds, card decline, or other errors.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Payment has been refunded to the customer.
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// Payment was cancelled before processing.
    /// </summary>
    Cancelled = 4
}
