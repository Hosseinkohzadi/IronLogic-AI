namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a financial transaction related to subscription purchases or renewals.
/// </summary>
public class PaymentTransaction : BaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who initiated the transaction.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property to the user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the unique transaction identifier from the payment gateway.
    /// </summary>
    public string GatewayTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the transaction (e.g., "Pending", "Completed", "Failed").
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
