using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
///     Controller exposing coaching endpoints backed by AI-style services.
/// </summary>
[ApiController]
[Route("api/v1/coach")]
public class CoachController : ControllerBase
{
    private readonly IWorkoutAnalysisService _analysisService;
    private readonly ICoachService _coachService;

    /// <summary>
    ///     Creates a new instance of <see cref="CoachController" />.
    /// </summary>
    /// <param name="coachService">Service that generates coaching advice.</param>
    /// <param name="analysisService">Service that computes physique analytics (e.g., V-Taper).</param>
    public CoachController(ICoachService coachService, IWorkoutAnalysisService analysisService)
    {
        _coachService = coachService ?? throw new ArgumentNullException(nameof(coachService));
        _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
    }

    /// <summary>
    ///     GET /api/v1/coach/analyze
    ///     Returns a mocked AI-driven coaching analysis. The endpoint uses <see cref="WorkoutAnalysisService" />
    ///     to compute a V-Taper (chest-to-waist ratio) and then delegates to <see cref="ICoachService" />
    ///     to generate a professional advice string.
    ///     Mock Phase: returns JSON payload containing advice wrapped in a <see cref="CoachAdviceResponse" />.
    /// </summary>
    /// <returns>A <see cref="CoachAdviceResponse" /> with the generated advice string.</returns>
    [HttpGet("analyze")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CoachAdviceResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoachAdviceResponse>> AnalyzeAsync()
    {
        // NOTE (mock): In a full implementation we'd fetch a user's latest MuscleMeasurement and training volume.
        // For the mock phase we create a representative measurement and volume.
        var measurement = new MuscleMeasurement
        {
            Date = DateTime.UtcNow.Date,
            Neck = 40.0,
            Chest = 110.0,
            Waist = 75.0
        };

        // Compute V-Taper (chest-to-waist ratio)
        var vTaper = _analysisService.CalculateChestToWaistRatio(measurement);

        // Mock monthly training volume (in pounds)
        double monthlyVolume = 120_000;

        // Generate advice from the coach service (string) and wrap in DTO
        var advice = await _coachService.GenerateAdviceAsync(vTaper, monthlyVolume);

        var response = new CoachAdviceResponse
        {
            Advice = advice
        };

        return Ok(response);
    }
}