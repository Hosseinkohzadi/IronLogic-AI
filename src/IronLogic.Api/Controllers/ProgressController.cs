using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

[ApiController]
[Route("api/v1/progress")]
[Tags("Progress")]
public class ProgressController(IDailyWeightService dailyWeightService) : ControllerBase
{
    /// <summary>
    /// Log daily weight.
    /// </summary>
    [HttpPost("weight")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogWeight([FromBody] DailyWeightRequest request)
    {
        var entry = await dailyWeightService.LogWeightAsync(request);

        return Created(string.Empty, entry);
    }
}