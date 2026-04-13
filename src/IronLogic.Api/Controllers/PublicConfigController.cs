using IronLogic.Application.DTOs.Settings;
using IronLogic.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Exposes safe public configuration values for client applications.
/// </summary>
[ApiController]
[Route("api/v1/public")]
[Produces("application/json")]
public class PublicConfigController(IPlatformSettingsService platformSettingsService) : ControllerBase
{
    /// <summary>
    /// Returns public pricing configuration for athlete-facing clients.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Safe pricing configuration values.</returns>
    [AllowAnonymous]
    [HttpGet("pricing-config")]
    [ProducesResponseType<PricingConfigDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPricingConfig(CancellationToken cancellationToken)
    {
        var config = await platformSettingsService.GetPublicPricingConfigAsync(cancellationToken);
        return Ok(config);
    }
}
