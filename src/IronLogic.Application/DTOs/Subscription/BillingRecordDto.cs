namespace IronLogic.Application.DTOs.Subscription;

/// <summary>
/// Unified billing record DTO combining subscription, transaction, and user information
/// </summary>
/// <param name="Id">Unique billing record identifier (transaction or subscription ID)</param>
/// <param name="UserEmail">Email address of the user</param>
/// <param name="PlanName">Name of the subscription plan</param>
/// <param name="Amount">Transaction amount</param>
/// <param name="Currency">Currency code (USD, CAD, EUR, GBP, AUD)</param>
/// <param name="Status">Payment status (Paid, Failed, Pending)</param>
/// <param name="TransactionDate">Date when the transaction occurred</param>
/// <param name="SubscriptionExpiry">Expiration date of the subscription</param>
public record BillingRecordDto(
    Guid Id,
    string UserEmail,
    string PlanName,
    decimal Amount,
    string Currency,
    string Status,
    DateTime TransactionDate,
    DateTime? SubscriptionExpiry);
