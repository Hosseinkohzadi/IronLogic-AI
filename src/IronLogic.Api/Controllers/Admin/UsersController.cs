using IronLogic.Application.DTOs.User;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    ILogger<UsersController> logger) : ControllerBase
{
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
        var user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            logger.LogWarning("User not found: {UserId}", id);
            return NotFound(new { message = "User not found" });
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

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
            Claims = claims.Select(c => new UserClaimDto
            {
                Type = c.Type,
                Value = c.Value
            }).ToList()
        };

        logger.LogInformation("Retrieved user details for: {UserId}", id);
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
}
