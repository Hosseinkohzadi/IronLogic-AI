using IronLogic.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

[ApiController]
[Route("api/v1/workouts")]
[Tags("Workouts")]
public class WorkoutController(IWorkoutService workoutService) : ControllerBase
{
    /// <summary>
    /// Get all workout sessions with exercises and sets.
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await workoutService.GetSessionsAsync();

        return Ok(sessions);
    }

    /// <summary>
    /// Get aggregate workout statistics. Volume = Weight * Reps.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var stats = await workoutService.GetStatsAsync();

        return Ok(stats);
    }
}