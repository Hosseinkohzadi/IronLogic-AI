using IronLogic.Application.DTOs.Financial;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;
using IronLogic.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Controller for financial operations including subscriptions, payments, and revenue statistics.
/// Integrates with Stripe for payment processing and multi-currency support.
/// </summary>
/// <param name="stripeService">Stripe payment service.</param>
/// <param name="subscriptionService">Subscription business logic service.</param>
/// <param name="paymentTransactionRepository">Payment transaction repository.</param>
/// <param name="userSubscriptionRepository">User subscription repository.</param>
[ApiController]
[Route("api/v1/financial")]
[Produces("application/json")]
public class FinancialController(
    IStripeService stripeService,
    ISubscriptionService subscriptionService,
    IGenericRepository<PaymentTransaction> paymentTransactionRepository,
    IGenericRepository<UserSubscription> userSubscriptionRepository) : ControllerBase
{
    /// <summary>
    /// Creates a Stripe Checkout Session for subscription purchase.
    /// Automatically applies Canadian GST/HST based on user's country and region.
    /// </summary>
    /// <param name="request">The checkout session request containing plan ID, user details, and location.</param>
    /// <returns>The Checkout Session ID for redirecting to Stripe Checkout.</returns>
    /// <response code="200">Returns the checkout session details.</response>
    /// <response code="400">Invalid request parameters.</response>
    [HttpPost("checkout/create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!Enum.TryParse<Currency>(request.currency, true, out var currency))
            return BadRequest(new { Message = $"Invalid currency: {request.currency}" });

        var sessionId = await stripeService.CreateCheckoutSessionAsync(
            request.planId,
            request.userEmail,
            request.userId,
            currency,
            request.countryCode,
            request.regionCode);

        return Ok(new CheckoutSessionResponse
        {
            sessionId = sessionId,
            checkoutUrl = $"https://checkout.stripe.com/pay/{sessionId}"
        });
    }

    /// <summary>
    /// Webhook endpoint for Stripe events.
    /// Processes: checkout.session.completed, invoice.paid, customer.subscription.deleted.
    /// </summary>
    /// <returns>200 OK if webhook processed successfully.</returns>
    /// <response code="200">Webhook processed successfully.</response>
    /// <response code="400">Webhook signature verification failed.</response>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        if (string.IsNullOrEmpty(stripeSignature))
            return BadRequest(new { Message = "Missing Stripe-Signature header" });

        var success = await stripeService.HandleWebhookAsync(json, stripeSignature);

        return success ? Ok() : BadRequest(new { Message = "Webhook processing failed" });
    }

    /// <summary>
    /// Retrieves aggregated revenue and subscription statistics for the Financial Dashboard.
    /// </summary>
    /// <param name="baseCurrency">The base currency for revenue aggregation (default: USD).</param>
    /// <returns>Revenue statistics including monthly revenue, active subscriptions, and churn rate.</returns>
    /// <response code="200">Returns the revenue statistics.</response>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<RevenueStatsDto>> GetRevenueStatsAsync([FromQuery] string baseCurrency = "USD")
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = currentMonthStart.AddMonths(-1);

        var allTransactions = await paymentTransactionRepository.ListAllAsync();
        var allSubscriptions = await userSubscriptionRepository.ListAllAsync();

        var currentMonthTransactions = allTransactions
            .Where(t => t.ProcessedAt >= currentMonthStart && t.Status == PaymentStatus.Completed)
            .ToList();

        var lastMonthTransactions = allTransactions
            .Where(t => t.ProcessedAt >= lastMonthStart && t.ProcessedAt < currentMonthStart && t.Status == PaymentStatus.Completed)
            .ToList();

        var monthlyRevenue = currentMonthTransactions.Sum(t => t.Amount);
        var lastMonthRevenue = lastMonthTransactions.Sum(t => t.Amount);

        var revenueGrowth = lastMonthRevenue > 0
            ? Math.Round((monthlyRevenue - lastMonthRevenue) / lastMonthRevenue * 100, 2)
            : 0m;

        var activeSubscriptions = allSubscriptions
            .Count(s => s.IsActive && s.EndDate > now);

        var pendingPayments = allTransactions
            .Count(t => t.Status == PaymentStatus.Pending);

        var totalSubscriptions = allSubscriptions.Count(s => s.StartDate >= lastMonthStart);
        var cancelledSubscriptions = allSubscriptions.Count(s => s.CancelledAt >= lastMonthStart && s.CancelledAt < now);
        var churnRate = totalSubscriptions > 0
            ? Math.Round((decimal)cancelledSubscriptions / totalSubscriptions * 100, 2)
            : 0m;

        var monthlyRevenueData = new List<MonthlyRevenueDto>();
        for (var i = 5; i >= 0; i--)
        {
            var monthStart = currentMonthStart.AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);

            var monthRevenue = allTransactions
                .Where(t => t.ProcessedAt >= monthStart && t.ProcessedAt < monthEnd && t.Status == PaymentStatus.Completed)
                .Sum(t => t.Amount);

            monthlyRevenueData.Add(new MonthlyRevenueDto
            {
                month = monthStart.ToString("MMM"),
                amount = monthRevenue
            });
        }

        return Ok(new RevenueStatsDto
        {
            monthlyRevenue = monthlyRevenue,
            yearlyRevenue = monthlyRevenue * 12,
            activeSubscriptions = activeSubscriptions,
            pendingPayments = pendingPayments,
            churnRate = churnRate,
            revenueGrowth = revenueGrowth,
            baseCurrency = baseCurrency,
            monthlyRevenueData = monthlyRevenueData
        });
    }

    /// <summary>
    /// Retrieves the active subscription for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The active subscription details.</returns>
    /// <response code="200">Returns the active subscription.</response>
    /// <response code="404">No active subscription found.</response>
    [HttpGet("subscription/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSubscription>> GetActiveSubscription(string userId)
    {
        var subscription = await subscriptionService.GetActiveSubscriptionAsync(userId);

        if (subscription == null)
            return NotFound(new { Message = "No active subscription found for this user" });

        return Ok(subscription);
    }

    /// <summary>
    /// Cancels a user's subscription at the end of the billing period.
    /// </summary>
    /// <param name="stripeSubscriptionId">The Stripe Subscription ID to cancel.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Subscription cancelled successfully.</response>
    /// <response code="400">Cancellation failed.</response>
    [HttpPost("subscription/{stripeSubscriptionId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelSubscription(string stripeSubscriptionId)
    {
        var success = await stripeService.CancelSubscriptionAsync(stripeSubscriptionId);

        if (!success)
            return BadRequest(new { Message = "Failed to cancel subscription" });

        await subscriptionService.DeactivateSubscriptionAsync(stripeSubscriptionId, "Cancelled by user");

        return Ok(new { Message = "Subscription cancelled successfully" });
    }
}
