using System.Security.Claims;

using IronLogic.Application.DTOs.Profile;
using IronLogic.Application.Interfaces;
using IronLogic.Application.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Handles authenticated account operations for the current user.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AccountController(IProfileService profileService) : ControllerBase
{
    /// <summary>
    /// Returns identity and profile data for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current user's identity and profile details.</returns>
    [HttpGet("me")]
    [ProducesResponseType<UserProfileResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }

        var result = await profileService.GetProfileAsync(userId, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates profile information for the currently authenticated user.
    /// </summary>
    /// <param name="request">Profile update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user profile details.</returns>
    [HttpPut("me")]
    [ProducesResponseType<UserProfileResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateProfileDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }

        Result<UserProfileResponseDto> result = await profileService.UpdateProfileAsync(userId, request, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(result.Value);
    }
}
