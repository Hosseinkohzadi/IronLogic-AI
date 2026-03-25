using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

[ApiController]
[Route("api/v1/workouts")]
[Tags("Workouts")]
public class WorkoutController(
    IWorkoutService workoutService,
    IWorkoutProvider workoutProvider,
    IWorkoutAnalyticsService analyticsService) : ControllerBase
{
    /// <summary>
    ///     Get all workout sessions with exercises and sets.
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await workoutService.GetSessionsAsync();

        return Ok(sessions);
    }

    /// <summary>
    ///     Get aggregate workout statistics for the most recent session.
    ///     Returns total volume, top exercise (by volume), intensity score,
    ///     and the session date.
    /// </summary>
    [HttpGet("stats")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(WorkoutStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetStats()
    {
        var recentSessions = (await workoutProvider.GetRecentSessionsAsync(1)).ToList();

        var lastSession = recentSessions.FirstOrDefault();

        if (lastSession is null)
            return NoContent();

        var totalVolume = analyticsService.CalculateTotalVolume(lastSession);
        var intensityScore = analyticsService.GetIntensityScore(lastSession);
        var topExercise = analyticsService.GetTopExercise(lastSession);

        var response = new WorkoutStatsResponse
        {
            TotalVolume = totalVolume,
            TopExercise = topExercise?.Name,
            IntensityScore = intensityScore,
            SessionDate = lastSession.StartTime
        };

        return Ok(response);
    }
}