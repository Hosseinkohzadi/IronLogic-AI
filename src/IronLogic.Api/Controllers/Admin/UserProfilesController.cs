using IronLogic.Application.DTOs.Profile;
using IronLogic.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers.Admin;

/// <summary>
/// Administrative endpoints for retrieving and updating profiles for any user.
/// </summary>
[ApiController]
[Route("api/v1/admin/profiles")]
[Authorize(Roles = "Admin")]
public class UserProfilesController(IProfileService profileService) : ControllerBase
{
    /// <summary>
    /// Returns identity and profile data for a specific user.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The target user's identity and profile details.</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType<UserProfileResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserId(string userId, CancellationToken cancellationToken)
    {
        var result = await profileService.GetProfileAsync(userId, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates profile data for a specific user.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="request">Profile update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated identity and profile details.</returns>
    [HttpPut("{userId}")]
    [ProducesResponseType<UserProfileResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateByUserId(
        string userId,
        [FromBody] UpdateProfileDto request,
        CancellationToken cancellationToken)
    {
        var result = await profileService.UpdateProfileAsync(userId, request, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(result.Value);
    }
}
