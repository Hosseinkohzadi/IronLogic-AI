using IronLogic.Application.DTOs;
using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
///     Manages workout sessions for users.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize]
public class WorkoutsController(
    IWorkoutSessionRepository repository,
    IWorkoutService workoutService) : ControllerBase
{
    private readonly Guid CurrentUserId = new("00000000-0000-0000-0000-000000000001");

    //private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User is not authenticated."));

    /// <summary>
    ///     Retrieves all workout sessions for the current user.
    /// </summary>
    /// <returns>A list of workout sessions.</returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<Session>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Session>>> GetWorkouts()
    {
        var sessions = await repository.GetAllByUserIdAsync(CurrentUserId.ToString());
        return Ok(sessions);
    }

    /// <summary>
    ///     Retrieves a specific workout session by its unique identifier.
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

        if (session == null || session.UserId != CurrentUserId.ToString()) 
            return NotFound();

        return Ok(session);
    }

    /// <summary>
    ///     Deletes a specific workout session.
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
        if (session == null || session.UserId != CurrentUserId.ToString()) 
            return NotFound();

        repository.Delete(session);
        await repository.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    ///     Retrieves workout statistics for the current user.
    /// </summary>
    /// <returns>A collection of workout statistics.</returns>
    [HttpGet("stats")]
    [ProducesResponseType<WorkoutStatsResponseDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkoutStatsResponseDto>> GetWorkoutStats()
    {
        var stats = await repository.GetWorkoutStatsAsync(CurrentUserId.ToString());
        return Ok(stats);
    }

    /// <summary>
    ///     Retrieves the weekly workout volume trend for the current user over the last 12 weeks.
    /// </summary>
    /// <returns>The weekly volume trend data.</returns>
    [HttpGet("weekly-trend")]
    [ProducesResponseType<IEnumerable<object>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetWeeklyVolumeTrend()
    {
        var twelveWeeksAgo = DateTime.UtcNow.AddDays(-84);

        var result = await repository.GetWeeklyVolumeTrend(CurrentUserId.ToString(), twelveWeeksAgo);

        return Ok(result);
    }


    /// <summary>
    ///     Parses a raw text workout log, creates the corresponding session and exercise entries,
    ///     and returns the structured, parsed data.
    /// </summary>
    /// <param name="request">The request containing the raw workout text.</param>
    /// <returns>
    ///     A 201 Created response with the location of the new session and the parsed workout data,
    ///     or a 400 Bad Request if the text is invalid or cannot be parsed.
    /// </returns>
    [HttpPost("import-text")]
    [ProducesResponseType<ParsedWorkoutDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWorkout([FromBody] WorkoutImportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.WorkoutText)) 
            return BadRequest("Workout text cannot be empty.");

        // result.Value now contains SessionId and ParsedData
        var result = await workoutService.CreateFromRawTextAsync(request.WorkoutText, CurrentUserId.ToString());

        if (result.IsFailure) 
            return BadRequest(new { message = result.Error });

        // Return the parsed data (ParsedData) to the front-end
        return CreatedAtAction(
            nameof(GetWorkout),
            new { id = result.Value.SessionId },
            result.Value.ParsedData
        );
    }
}