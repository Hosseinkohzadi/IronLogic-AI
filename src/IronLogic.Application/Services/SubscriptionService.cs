using System.Text.Json;
using IronLogic.Application.DTOs.Subscription;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;
using IronLogic.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace IronLogic.Application.Services;

/// <summary>
/// Implements subscription management operations including plan retrieval and user subscription creation
/// </summary>
public class SubscriptionService(
    IGenericRepository<SubscriptionPlan> planRepository,
    IGenericRepository<UserSubscription> subscriptionRepository,
    IGenericRepository<PaymentTransaction> transactionRepository,
    ILogger<SubscriptionService> logger) : ISubscriptionService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetAvailablePlansAsync()
    {
        logger.LogInformation("Retrieving available subscription plans");

        // For now, return hardcoded plans as requested
        // In production, this would query the database
        var plans = new List<SubscriptionPlanDto>
        {
            new SubscriptionPlanDto(
                Id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name: "Basic",
                Price: 0m,
                Currency: "USD",
                Description: "Free forever - Perfect for getting started",
                Features: new List<string>
                {
                    "Track unlimited workouts",
                    "Basic exercise library",
                    "Progress tracking",
                    "Personal records"
                }
            ),
            new SubscriptionPlanDto(
                Id: Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name: "Pro",
                Price: 29m,
                Currency: "USD",
                Description: "Most popular - For serious athletes",
                Features: new List<string>
                {
                    "Everything in Basic",
                    "AI workout insights",
                    "Advanced analytics",
                    "Custom exercise creation",
                    "Export workout data",
                    "Priority support"
                }
            ),
            new SubscriptionPlanDto(
                Id: Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name: "Elite",
                Price: 99m,
                Currency: "USD",
                Description: "Ultimate experience - For competitive athletes",
                Features: new List<string>
                {
                    "Everything in Pro",
                    "Personal coach AI advisor",
                    "Video form analysis",
                    "Competition tracking",
                    "Nutrition planning",
                    "White-label branding",
                    "API access",
                    "Dedicated support"
                }
            )
        };

        logger.LogInformation("Retrieved {Count} subscription plans", plans.Count);
        return plans.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<SubscriptionResponseDto> SubscribeAsync(string userId, Guid planId, string paymentMethodId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(paymentMethodId))
            throw new ArgumentException("Payment method ID cannot be null or empty", nameof(paymentMethodId));

        logger.LogInformation(
            "Processing subscription for User: {UserId}, Plan: {PlanId}, PaymentMethod: {PaymentMethodId}",
            userId, planId, paymentMethodId);

        // Placeholder logic: Generate fake transaction ID
        var transactionId = $"TXN_{Guid.NewGuid():N}";
        var subscriptionId = Guid.NewGuid();

        // TODO: Integrate with payment gateway (Stripe)
        // TODO: Create UserSubscription entity
        // TODO: Create PaymentTransaction entity
        // TODO: Activate subscription

        logger.LogInformation(
            "Subscription successful - User: {UserId}, Transaction: {TransactionId}, Subscription: {SubscriptionId}",
            userId, transactionId, subscriptionId);

        return new SubscriptionResponseDto(
            Success: true,
            Message: "Subscription created successfully. Payment processing initiated.",
            TransactionId: transactionId,
            SubscriptionId: subscriptionId
        );
    }

    /// <inheritdoc />
    public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        logger.LogInformation("Retrieving active subscription for User: {UserId}", userId);

        // TODO: Query database for active subscription
        // For now, return null (no subscription)
        return null;
    }

    /// <inheritdoc />
    public async Task<SubscriptionPlanDto> CreatePlanAsync(CreatePlanDto createDto)
    {
        logger.LogInformation("Creating new subscription plan: {PlanName}", createDto.Name);

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Price = createDto.Price,
            Currency = Enum.Parse<Currency>(createDto.Currency),
            DurationDays = createDto.DurationDays,
            FeaturesJson = JsonSerializer.Serialize(createDto.Features),
            IsActive = true
        };

        await planRepository.AddAsync(plan);
        await planRepository.SaveChangesAsync();

        logger.LogInformation("Successfully created plan: {PlanId} - {PlanName}", plan.Id, plan.Name);

        return new SubscriptionPlanDto(
            plan.Id,
            plan.Name,
            plan.Price,
            plan.Currency.ToString(),
            createDto.Description ?? string.Empty,
            createDto.Features
        );
    }

    /// <inheritdoc />
    public async Task<SubscriptionPlanDto?> UpdatePlanAsync(Guid planId, UpdatePlanDto updateDto)
    {
        logger.LogInformation("Updating subscription plan: {PlanId}", planId);

        var plan = await planRepository.GetByIdAsync(planId);
        
        if (plan == null)
        {
            logger.LogWarning("Plan not found: {PlanId}", planId);
            return null;
        }

        // Update only provided fields
        if (updateDto.Name != null)
            plan.Name = updateDto.Name;

        if (updateDto.Price.HasValue)
            plan.Price = updateDto.Price.Value;

        if (updateDto.Currency != null)
            plan.Currency = Enum.Parse<Currency>(updateDto.Currency);

        if (updateDto.DurationDays.HasValue)
            plan.DurationDays = updateDto.DurationDays.Value;

        if (updateDto.Features != null)
            plan.FeaturesJson = JsonSerializer.Serialize(updateDto.Features);

        if (updateDto.IsActive.HasValue)
            plan.IsActive = updateDto.IsActive.Value;

        planRepository.Update(plan);
        await planRepository.SaveChangesAsync();

        logger.LogInformation("Successfully updated plan: {PlanId} - {PlanName}", plan.Id, plan.Name);

        var features = string.IsNullOrEmpty(plan.FeaturesJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(plan.FeaturesJson) ?? new List<string>();

        return new SubscriptionPlanDto(
            plan.Id,
            plan.Name,
            plan.Price,
            plan.Currency.ToString(),
            updateDto.Description ?? string.Empty,
            features
        );
    }

    /// <inheritdoc />
    public async Task<bool> DeletePlanAsync(Guid planId)
    {
        logger.LogInformation("Soft deleting subscription plan: {PlanId}", planId);

        var plan = await planRepository.GetByIdAsync(planId);
        
        if (plan == null)
        {
            logger.LogWarning("Plan not found: {PlanId}", planId);
            return false;
        }

        // Soft delete: Mark as inactive instead of physical delete
        // This preserves data integrity for existing subscriptions
        plan.IsActive = false;
        
        planRepository.Update(plan);
        await planRepository.SaveChangesAsync();

        logger.LogInformation("Successfully soft deleted plan: {PlanId} - {PlanName}", plan.Id, plan.Name);

        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentTransactionDto>> GetAllTransactionsAsync()
    {
        logger.LogInformation("Retrieving all payment transactions for admin");

        // TODO: Implement actual query with user joins
        // For now, return empty list as placeholder
        logger.LogInformation("Retrieved 0 transactions (placeholder implementation)");
        
        return new List<PaymentTransactionDto>().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BillingRecordDto>> GetBillingRecordsAsync()
    {
        logger.LogInformation("Retrieving unified billing records for admin");

        // TODO: Implement actual database query with joins
        // This will join UserSubscriptions, SubscriptionPlans, PaymentTransactions, and Users
        // For now, return empty list as placeholder
        
        /*
        Example implementation (when database is ready):
        
        var billingRecords = await (
            from subscription in subscriptionRepository.GetAllWithIncludes()
            join plan in planRepository.GetAll() on subscription.PlanId equals plan.Id
            join user in userRepository.GetAll() on subscription.UserId equals user.Id
            join transaction in transactionRepository.GetAll() on subscription.StripeSubscriptionId equals transaction.StripeSubscriptionId into transactions
            from transaction in transactions.DefaultIfEmpty()
            select new BillingRecordDto(
                Id: subscription.Id,
                UserEmail: user.Email,
                PlanName: plan.Name,
                Amount: plan.Price,
                Currency: plan.Currency.ToString(),
                Status: subscription.IsActive && subscription.EndDate > DateTime.UtcNow ? "Paid" : "Expired",
                TransactionDate: subscription.StartDate,
                SubscriptionExpiry: subscription.EndDate
            )
        ).ToListAsync();
        */

        logger.LogInformation("Retrieved 0 billing records (placeholder implementation)");
        
        return new List<BillingRecordDto>().AsReadOnly();
    }
}


