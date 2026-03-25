namespace IronLogic.Api.Controllers;

/// <summary>
///     Controller exposing coaching endpoints backed by AI-style services.
/// </summary>
[ApiController]
[Route("api/v1/coach")]
public class CoachController(ICoachService coachService) : ControllerBase
{
    /// <summary>
    ///     GET /api/v1/coach/analyze
    ///     Returns an AI-driven coaching analysis. Uses Semantic Kernel to generate advice
    ///     from the athlete's latest workout stats and body metrics. Falls back to rule-based
    ///     advice if the AI service is unavailable.
    /// </summary>
    /// <returns>A <see cref="CoachAdviceResponse" /> with the generated advice string.</returns>
    [HttpGet("analyze")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CoachAdviceResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoachAdviceResponse>> AnalyzeAsync()
    {
        var advice = await coachService.AnalyzeAsync();

        var response = new CoachAdviceResponse
        {
            Advice = advice
        };

        return Ok(response);
    }
}