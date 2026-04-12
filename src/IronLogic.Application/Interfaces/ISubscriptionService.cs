using IronLogic.Application.DTOs.Subscription;

namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines operations for managing subscription plans and user subscriptions
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Retrieves all available subscription plans
    /// </summary>
    /// <returns>A read-only list of subscription plan DTOs</returns>
    Task<IReadOnlyList<SubscriptionPlanDto>> GetAvailablePlansAsync();

    /// <summary>
    /// Creates a subscription for a user with the specified plan and payment method
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="planId">The unique identifier of the subscription plan</param>
    /// <param name="paymentMethodId">The payment method identifier from payment gateway</param>
    /// <returns>A subscription response containing transaction details</returns>
    Task<SubscriptionResponseDto> SubscribeAsync(string userId, Guid planId, string paymentMethodId);

    /// <summary>
    /// Retrieves the active subscription for a specific user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>The active user subscription, or null if no active subscription exists</returns>
    Task<Domain.Entities.UserSubscription?> GetActiveSubscriptionAsync(string userId);

    /// <summary>
    /// Creates a new subscription plan (Admin only)
    /// </summary>
    /// <param name="createDto">The plan creation data</param>
    /// <returns>The created subscription plan DTO</returns>
    Task<SubscriptionPlanDto> CreatePlanAsync(CreatePlanDto createDto);

    /// <summary>
    /// Updates an existing subscription plan (Admin only)
    /// </summary>
    /// <param name="planId">The unique identifier of the plan to update</param>
    /// <param name="updateDto">The plan update data</param>
    /// <returns>The updated subscription plan DTO, or null if plan not found</returns>
    Task<SubscriptionPlanDto?> UpdatePlanAsync(Guid planId, UpdatePlanDto updateDto);

    /// <summary>
    /// Soft deletes a subscription plan by marking it as inactive (Admin only)
    /// </summary>
    /// <param name="planId">The unique identifier of the plan to delete</param>
    /// <returns>True if the plan was successfully deactivated; otherwise, false</returns>
    Task<bool> DeletePlanAsync(Guid planId);

    /// <summary>
    /// Retrieves all payment transactions with user details (Admin only)
    /// </summary>
    /// <returns>A read-only list of payment transaction DTOs with user information</returns>
    Task<IReadOnlyList<PaymentTransactionDto>> GetAllTransactionsAsync();

    /// <summary>
    /// Retrieves unified billing records combining subscriptions, transactions, and user data (Admin only)
    /// </summary>
    /// <returns>A read-only list of billing record DTOs</returns>
    Task<IReadOnlyList<BillingRecordDto>> GetBillingRecordsAsync();
}


