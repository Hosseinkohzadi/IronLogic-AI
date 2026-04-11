using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;
using IronLogic.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace IronLogic.Application.Services;

/// <summary>
/// Implements business logic for managing user subscriptions.
/// Handles subscription activation, renewal, cancellation, and payment tracking.
/// All timestamps use UTC for consistent timezone handling across global users.
/// </summary>
/// <param name="userSubscriptionRepository">User subscription repository.</param>
/// <param name="paymentTransactionRepository">Payment transaction repository.</param>
/// <param name="planRepository">Subscription plan repository.</param>
/// <param name="logger">Logger for tracking subscription operations.</param>
public class SubscriptionService(
    IGenericRepository<UserSubscription> userSubscriptionRepository,
    IGenericRepository<PaymentTransaction> paymentTransactionRepository,
    IGenericRepository<SubscriptionPlan> planRepository,
    ILogger<SubscriptionService> logger) : ISubscriptionService
{
    /// <inheritdoc />
    public async Task<UserSubscription> ActivateSubscriptionAsync(
        string userId,
        Guid planId,
        string stripeSubscriptionId,
        string stripeCustomerId,
        decimal amount,
        decimal taxAmount,
        Currency currency,
        string countryCode,
        string? regionCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeSubscriptionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeCustomerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        var plan = await planRepository.GetByIdAsync(planId);
        if (plan == null)
            throw new InvalidOperationException($"Subscription plan with ID {planId} not found.");

        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(plan.DurationDays);

        var subscription = new UserSubscription
        {
            UserId = userId,
            PlanId = planId,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            AutoRenew = true,
            StripeSubscriptionId = stripeSubscriptionId,
            StripeCustomerId = stripeCustomerId
        };

        await userSubscriptionRepository.AddAsync(subscription);
        await userSubscriptionRepository.SaveChangesAsync();

        await RecordPaymentAsync(
            userId,
            amount,
            taxAmount,
            currency,
            stripeSubscriptionId,
            stripeSubscriptionId,
            null,
            countryCode,
            regionCode);

        logger.LogInformation(
            "Subscription activated for user {UserId}, plan {PlanId}, Stripe subscription {StripeSubscriptionId}",
            userId, planId, stripeSubscriptionId);

        return subscription;
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateSubscriptionAsync(string stripeSubscriptionId, string? cancellationReason = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeSubscriptionId);

        var subscriptions = await userSubscriptionRepository.ListAllAsync();
        var subscription = subscriptions.FirstOrDefault(s => s.StripeSubscriptionId == stripeSubscriptionId);

        if (subscription == null)
        {
            logger.LogWarning("Subscription not found for Stripe subscription ID: {StripeSubscriptionId}", stripeSubscriptionId);
            return false;
        }

        subscription.IsActive = false;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.CancellationReason = cancellationReason;

        userSubscriptionRepository.Update(subscription);
        await userSubscriptionRepository.SaveChangesAsync();

        logger.LogInformation(
            "Subscription deactivated for user {UserId}, Stripe subscription {StripeSubscriptionId}, reason: {Reason}",
            subscription.UserId, stripeSubscriptionId, cancellationReason ?? "Not specified");

        return true;
    }

    /// <inheritdoc />
    public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var subscriptions = await userSubscriptionRepository.ListAllAsync();
        return subscriptions
            .Where(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserSubscription>> GetUserSubscriptionsAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var subscriptions = await userSubscriptionRepository.ListAllAsync();
        return subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartDate)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<bool> HasActiveSubscriptionAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var subscription = await GetActiveSubscriptionAsync(userId);
        return subscription != null;
    }

    /// <inheritdoc />
    public async Task<PaymentTransaction> RecordPaymentAsync(
        string userId,
        decimal amount,
        decimal taxAmount,
        Currency currency,
        string gatewayTransactionId,
        string? stripeSubscriptionId = null,
        string? stripeInvoiceId = null,
        string? countryCode = null,
        string? regionCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(gatewayTransactionId);

        var transaction = new PaymentTransaction
        {
            UserId = userId,
            Amount = amount,
            TaxAmount = taxAmount,
            Currency = currency,
            GatewayTransactionId = gatewayTransactionId,
            StripeSubscriptionId = stripeSubscriptionId,
            StripeInvoiceId = stripeInvoiceId,
            CountryCode = countryCode ?? "US",
            RegionCode = regionCode,
            Status = PaymentStatus.Completed,
            PaymentMethod = "card",
            ProcessedAt = DateTime.UtcNow
        };

        await paymentTransactionRepository.AddAsync(transaction);
        await paymentTransactionRepository.SaveChangesAsync();

        logger.LogInformation(
            "Payment recorded: {Amount} {Currency} (tax: {TaxAmount}) for user {UserId}, transaction {TransactionId}",
            amount, currency, taxAmount, userId, gatewayTransactionId);

        return transaction;
    }
}
