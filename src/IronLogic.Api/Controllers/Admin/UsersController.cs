using IronLogic.Application.DTOs.Communication;
using IronLogic.Application.DTOs.User;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IronLogic.Api.Controllers.Admin;

/// <summary>
/// Administrative controller for managing users
/// </summary>
[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class UsersController(
    UserManager<User> userManager,
    IEmailService emailService,
    IAdminService adminService,
    ILogger<UsersController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves administrative metrics for user management dashboard
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User metrics including premium subscribers, active users, sessions, and churn risk</returns>
    /// <response code="200">Returns the user metrics</response>
    [HttpGet("metrics")]
    [ProducesResponseType<AdminUserMetricsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserMetrics(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving user metrics for admin dashboard");

        var metrics = await adminService.GetUserMetricsAsync(cancellationToken);

        logger.LogInformation(
            "Retrieved user metrics: Premium={Premium}, WAU={WAU}, Sessions={Sessions}, Churn={Churn}",
            metrics.PremiumSubscribers,
            metrics.WeeklyActiveUsers,
            metrics.TotalSessions,
            metrics.ChurnRiskCount);

        return Ok(metrics);
    }

    /// <summary>
    /// Retrieves all users with their roles and subscription information
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all users</returns>
    /// <response code="200">Returns the list of users</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AdminUserListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all users for admin");

        var users = userManager.Users
            .Include(u => u.Profile)
            .Include(u => u.UserSubscriptions)
                .ThenInclude(s => s.Plan)
            .ToList();

        var userList = new List<AdminUserListDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "User";

            var subscription = user.UserSubscriptions
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();

            string plan = "Basic";
            string status = "Expired";
            DateTimeOffset? subscriptionEndDate = null;

            if (subscription != null)
            {
                plan = subscription.Plan?.Name ?? "Basic";
                subscriptionEndDate = subscription.EndDate;
                status = subscription.EndDate >= DateTimeOffset.UtcNow ? "Active" : "Expired";
            }

            userList.Add(new AdminUserListDto
            {
                Id = user.Id,
                FirstName = user.Profile?.FirstName ?? user.UserName?.Split('@')[0] ?? "User",
                LastName = user.Profile?.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = primaryRole,
                Plan = plan,
                Status = status,
                SubscriptionEndDate = subscriptionEndDate,
                ProfileImageUrl = user.Profile?.ProfilePictureUrl ?? string.Empty
            });
        }

        logger.LogInformation("Retrieved {Count} users for admin", userList.Count);
        return Ok(userList);
    }

    /// <summary>
    /// Retrieves detailed information about a specific user including claims, roles, and lockout status
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed user information</returns>
    /// <response code="200">Returns the user details</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType<UserDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(string id, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .Include(u => u.Profile)
            .Include(u => u.Sessions)
            .Include(u => u.DailyWeights)
            .Include(u => u.UserSubscriptions)
                .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            logger.LogWarning("User not found: {UserId}", id);
            return NotFound(new { message = "User not found" });
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        var isActive = !(user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow);
        
        var lastSession = user.Sessions
            .OrderByDescending(s => s.Date)
            .FirstOrDefault();
        
        var activeSubscription = user.UserSubscriptions
            .Where(s => s.IsActive && s.EndDate >= DateTime.UtcNow)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefault();

        var subscriptionTier = activeSubscription?.Plan?.Name ?? "Free";
        var subscriptionStatus = activeSubscription != null ? "Active" : "Inactive";

        var userDetail = new UserDetailDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount,
            Roles = roles.ToList(),
            IsActive = isActive,
            LastLoginDate = lastSession?.Date,
            TotalSessions = user.Sessions.Count,
            TotalDailyWeights = user.DailyWeights.Count,
            ProfileImageUrl = user.Profile?.ProfilePictureUrl,
            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            SubscriptionTier = subscriptionTier,
            SubscriptionStatus = subscriptionStatus,
            Claims = claims.Select(c => new UserClaimDto
            {
                Type = c.Type,
                Value = c.Value
            }).ToList()
        };

        logger.LogInformation(
            "Retrieved user details for: {UserId} - IsActive: {IsActive}, Sessions: {Sessions}, LastLogin: {LastLogin}",
            id,
            isActive,
            user.Sessions.Count,
            lastSession?.Date);

        return Ok(userDetail);
    }

    /// <summary>
    /// Updates user information including email, name, and roles
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="updateDto">The user update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user information</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid user data or identity operation failed</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType<object>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(
        string id,
        [FromBody] UpdateUserDto updateDto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid user data", errors = ModelState });
        }

        var user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            logger.LogWarning("User not found for update: {UserId}", id);
            return NotFound(new { message = "User not found" });
        }

        var updateResult = IdentityResult.Success;

        // Update email if provided
        if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != user.Email)
        {
            var setEmailResult = await userManager.SetEmailAsync(user, updateDto.Email);
            if (!setEmailResult.Succeeded)
            {
                logger.LogWarning(
                    "Failed to update email for user {UserId}: {Errors}",
                    id,
                    string.Join(", ", setEmailResult.Errors.Select(e => e.Description)));
                return BadRequest(new
                {
                    message = "Failed to update email",
                    errors = setEmailResult.Errors.Select(e => e.Description).ToList()
                });
            }

            // Also update username to match email
            var setUserNameResult = await userManager.SetUserNameAsync(user, updateDto.Email);
            if (!setUserNameResult.Succeeded)
            {
                logger.LogWarning(
                    "Failed to update username for user {UserId}: {Errors}",
                    id,
                    string.Join(", ", setUserNameResult.Errors.Select(e => e.Description)));
            }
        }

        // Update roles if provided
        if (updateDto.Roles != null)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(updateDto.Roles).ToList();
            var rolesToAdd = updateDto.Roles.Except(currentRoles).ToList();

            // Remove old roles
            if (rolesToRemove.Any())
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    logger.LogWarning(
                        "Failed to remove roles from user {UserId}: {Errors}",
                        id,
                        string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    return BadRequest(new
                    {
                        message = "Failed to update roles",
                        errors = removeResult.Errors.Select(e => e.Description).ToList()
                    });
                }
            }

            // Add new roles
            if (rolesToAdd.Any())
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    logger.LogWarning(
                        "Failed to add roles to user {UserId}: {Errors}",
                        id,
                        string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    return BadRequest(new
                    {
                        message = "Failed to update roles",
                        errors = addResult.Errors.Select(e => e.Description).ToList()
                    });
                }
            }

            logger.LogInformation(
                "Updated roles for user {UserId}: Removed [{RemovedRoles}], Added [{AddedRoles}]",
                id,
                string.Join(", ", rolesToRemove),
                string.Join(", ", rolesToAdd));
        }

        logger.LogInformation("Successfully updated user: {UserId}", id);

        return Ok(new
        {
            message = "User updated successfully",
            userId = user.Id,
            email = user.Email,
            userName = user.UserName
        });
    }

    /// <summary>
    /// Sends a manual custom email message to a specific user.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="request">The email payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or error response.</returns>
    [HttpPost("{userId}/send-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SendEmailToUser(
        string userId,
        [FromBody] SendEmailRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid email payload", errors = ModelState });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        try
        {
            await emailService.SendAndLogEmailAsync(
                userId,
                request.Subject,
                request.Body,
                isManual: true,
                cancellationToken);

            return Ok(new { message = "Email sent successfully" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Manual email delivery failed for user {UserId}", userId);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Email server is unavailable. Try again later."
            });
        }
    }
}
