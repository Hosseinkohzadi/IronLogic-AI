using IronLogic.Application.DTOs.Settings;
using IronLogic.Application.Interfaces;
using IronLogic.Application.DTOs.Communication;

using Hangfire;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers.Admin;

/// <summary>
/// Administrative controller for platform-level settings managed by SuperAdmins.
/// </summary>
[ApiController]
[Route("api/v1/admin/settings")]
[Authorize(Roles = "SuperAdmin")]
[Produces("application/json")]
public class SettingsController(
    IPlatformSettingsService platformSettingsService,
    IBackgroundJobClient backgroundJobClient) : ControllerBase
{
    /// <summary>
    /// Returns all platform settings.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All platform settings.</returns>
    [HttpGet("platform")]
    [ProducesResponseType<IReadOnlyList<PlatformSettingDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlatformSettings(CancellationToken cancellationToken)
    {
        var result = await platformSettingsService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates a platform setting value by key.
    /// </summary>
    /// <param name="key">Setting key.</param>
    /// <param name="request">New setting value payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated setting.</returns>
    [HttpPut("platform/{key}")]
    [ProducesResponseType<PlatformSettingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePlatformSetting(
        string key,
        [FromBody] UpdatePlatformSettingDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await platformSettingsService.UpdateValueAsync(key, request.Value, cancellationToken);
        if (updated is null)
            return NotFound(new { message = $"Platform setting '{key}' was not found." });

        return Ok(updated);
    }

    /// <summary>
    /// Starts a discount-offer broadcast campaign for all users.
    /// </summary>
    /// <param name="request">Broadcast campaign request payload.</param>
    /// <returns>Queued job information.</returns>
    [HttpPost("platform/broadcast-discount")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult BroadcastDiscountOffer([FromBody] BroadcastDiscountOfferRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var jobId = backgroundJobClient.Enqueue<IEmailAutomationService>(service =>
            service.QueueDiscountOfferBroadcastAsync(
                request.Subject,
                request.DiscountPercentage,
                request.CustomMessage,
                request.CallToActionUrl,
                CancellationToken.None));

        return Accepted(new { message = "Discount broadcast job queued.", jobId });
    }
}
