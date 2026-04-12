using System.Security.Claims;
using IronLogic.Application.DTOs.Subscription;
using IronLogic.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Controller for managing subscription plans and user subscriptions
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SubscriptionController(
    ISubscriptionService subscriptionService,
    ILogger<SubscriptionController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves all available subscription plans
    /// </summary>
    /// <returns>List of subscription plans with details and features</returns>
    /// <response code="200">Returns the list of available subscription plans</response>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans()
    {
        logger.LogInformation("Fetching available subscription plans");

        var plans = await subscriptionService.GetAvailablePlansAsync();

        return Ok(plans);
    }

    /// <summary>
    /// Creates a new subscription for the authenticated user
    /// </summary>
    /// <param name="request">Subscription request containing plan ID and payment method</param>
    /// <returns>Subscription response with transaction details</returns>
    /// <response code="200">Subscription created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User is not authenticated</response>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("Subscribe attempt without authenticated user");
            return Unauthorized(new { Message = "User is not authenticated" });
        }

        logger.LogInformation(
            "Subscribe request from User: {UserId}, Plan: {PlanId}",
            userId, request.PlanId);

        try
        {
            var response = await subscriptionService.SubscribeAsync(
                userId,
                request.PlanId,
                request.PaymentMethodId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid subscription request from User: {UserId}", userId);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing subscription for User: {UserId}", userId);
            return StatusCode(500, new { Message = "An error occurred while processing your subscription" });
        }
    }

    /// <summary>
    /// Retrieves the active subscription for the authenticated user
    /// </summary>
    /// <returns>Active subscription details if exists</returns>
    /// <response code="200">Returns the active subscription or null if no active subscription</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet("my-subscription")]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("GetMySubscription attempt without authenticated user");
            return Unauthorized(new { Message = "User is not authenticated" });
        }

        logger.LogInformation("Retrieving active subscription for User: {UserId}", userId);

        var subscription = await subscriptionService.GetActiveSubscriptionAsync(userId);

        if (subscription == null)
        {
            return Ok(new { Message = "No active subscription found", Subscription = (object?)null });
        }

        return Ok(subscription);
    }

    /// <summary>
    /// Creates a new subscription plan (Admin only)
    /// </summary>
    /// <param name="createDto">Plan creation data</param>
    /// <returns>The created subscription plan</returns>
    /// <response code="201">Plan created successfully</response>
    /// <response code="400">Invalid plan data</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (requires Admin role)</response>
    [HttpPost("admin/plans")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto createDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid plan creation request");
            return BadRequest(new { message = "Invalid plan data", errors = ModelState });
        }

        logger.LogInformation("Admin creating new plan: {PlanName}", createDto.Name);

        var plan = await subscriptionService.CreatePlanAsync(createDto);

        return CreatedAtAction(
            nameof(GetPlans),
            new { id = plan.Id },
            new { message = "Plan created successfully", data = plan });
    }

    /// <summary>
    /// Updates an existing subscription plan (Admin only)
    /// </summary>
    /// <param name="id">The plan ID to update</param>
    /// <param name="updateDto">Plan update data</param>
    /// <returns>The updated subscription plan</returns>
    /// <response code="200">Plan updated successfully</response>
    /// <response code="400">Invalid plan data</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (requires Admin role)</response>
    /// <response code="404">Plan not found</response>
    [HttpPut("admin/plans/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdatePlanDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid plan update request for Plan: {PlanId}", id);
            return BadRequest(new { message = "Invalid plan data", errors = ModelState });
        }

        logger.LogInformation("Admin updating plan: {PlanId}", id);

        var plan = await subscriptionService.UpdatePlanAsync(id, updateDto);

        if (plan == null)
        {
            logger.LogWarning("Plan not found for update: {PlanId}", id);
            return NotFound(new { message = "Plan not found" });
        }

        return Ok(new { message = "Plan updated successfully", data = plan });
    }

    /// <summary>
    /// Deletes a subscription plan (Admin only) - Soft delete to preserve subscription history
    /// </summary>
    /// <param name="id">The plan ID to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">Plan deleted successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (requires Admin role)</response>
    /// <response code="404">Plan not found</response>
    [HttpDelete("admin/plans/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePlan(Guid id)
    {
        logger.LogInformation("Admin deleting plan: {PlanId}", id);

        var success = await subscriptionService.DeletePlanAsync(id);

        if (!success)
        {
            logger.LogWarning("Plan not found for deletion: {PlanId}", id);
            return NotFound(new { message = "Plan not found" });
        }

        logger.LogInformation("Successfully deleted plan: {PlanId}", id);
        return Ok(new { message = "Plan deleted successfully" });
    }

    /// <summary>
    /// Retrieves all payment transactions with user details (Admin only)
    /// </summary>
    /// <returns>List of all payment transactions with user information</returns>
    /// <response code="200">Returns the list of payment transactions</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (requires Admin role)</response>
    [HttpGet("admin/all-transactions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions()
    {
        logger.LogInformation("Admin retrieving all payment transactions");

        var transactions = await subscriptionService.GetAllTransactionsAsync();

        logger.LogInformation("Retrieved {Count} transactions", transactions.Count);
        return Ok(new { message = "Transactions retrieved successfully", count = transactions.Count, data = transactions });
    }

    /// <summary>
    /// Retrieves unified billing records combining subscriptions, transactions, and user data (Admin only)
    /// </summary>
    /// <returns>List of unified billing records</returns>
    /// <response code="200">Returns the list of billing records</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User is not authorized (requires Admin role)</response>
    [HttpGet("admin/billing-records")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetBillingRecords()
    {
        logger.LogInformation("Admin retrieving unified billing records");

        var billingRecords = await subscriptionService.GetBillingRecordsAsync();

        logger.LogInformation("Retrieved {Count} billing records", billingRecords.Count);
        return Ok(new { message = "Billing records retrieved successfully", count = billingRecords.Count, data = billingRecords });
    }
}



