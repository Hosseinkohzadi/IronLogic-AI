using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Manages workout sessions for users.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize]
public class WorkoutsController(IWorkoutSessionRepository repository) : ControllerBase
{
    private readonly string CurrentUserId = "00000000-0000-0000-0000-000000000001";

    //private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User is not authenticated."));

    /// <summary>
    /// Retrieves all workout sessions for the current user.
    /// </summary>
    /// <returns>A list of workout sessions.</returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<Session>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Session>>> GetWorkouts()
    {
        var sessions = await repository.GetAllByUserIdAsync(CurrentUserId);
        return Ok(sessions);
    }

    /// <summary>
    /// Retrieves a specific workout session by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the workout session to retrieve.</param>
    /// <returns>The requested workout session.</returns>
    /// <response code="200">Returns the requested workout session.</response>
    /// <response code="404">If the workout session with the specified ID is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Session>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Session>> GetWorkout(Guid id)
    {
        var session = await repository.GetByIdAsync(id);

        if (session == null || session.UserId != CurrentUserId)
            return NotFound();

        return Ok(session);
    }

    /// <summary>
    /// Deletes a specific workout session.
    /// </summary>
    /// <param name="id">The ID of the workout session to delete.</param>
    /// <returns>An empty response indicating success.</returns>
    /// <response code="204">Indicates the workout session was successfully deleted.</response>
    /// <response code="404">If the workout session with the specified ID is not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkout(Guid id)
    {
        var session = await repository.GetByIdAsync(id);
        if (session == null || session.UserId != CurrentUserId)
            return NotFound();

        repository.Delete(session);
        await repository.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Retrieves workout statistics for the current user.
    /// </summary>
    /// <returns>A collection of workout statistics.</returns>
    [HttpGet("stats")]
    [ProducesResponseType<IEnumerable<WorkoutStatsResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkoutStatsResponseDto>>> GetWorkoutStats()
    {
        var stats = await repository.GetWorkoutStatsAsync(CurrentUserId);
        return Ok(stats);
    }

    /// <summary>
    /// Retrieves the weekly workout volume trend for the current user over the last 12 weeks.
    /// </summary>
    /// <returns>The weekly volume trend data.</returns>
    [HttpGet("weekly-trend")]
    [ProducesResponseType<IEnumerable<object>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetWeeklyVolumeTrend()
    {
        var twelveWeeksAgo = DateTime.UtcNow.AddDays(-84);

        var result = await repository.GetWeeklyVolumeTrend(CurrentUserId, twelveWeeksAgo);

        return Ok(result);
    }

    /// <summary>
    /// Creates a new workout session for the current user.
    /// </summary>
    /// <param name="workoutSessionDto">The details of the workout session to create.</param>
    /// <returns>The newly created workout session.</returns>
    /// <response code="201">Returns the newly created workout session.</response>
    /// <response code="400">If the workout session data is invalid.</response>
    [HttpPost]
    [ProducesResponseType<Session>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Session>> CreateWorkout(string workoutSessionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        //todo: implement parsing logic to convert raw string to Session entity and pass to service layer for creation.This is a placeholder for now.

        return null;
    }
}