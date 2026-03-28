using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Manages workout sessions for users.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class WorkoutsController(IWorkoutSessionRepository repository) : ControllerBase
{
    private readonly Guid _currentUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// Retrieves all workout sessions for the current user.
    /// </summary>
    /// <returns>A list of workout sessions.</returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<Session>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Session>>> GetWorkouts()
    {
        var sessions = await repository.GetAllByUserIdAsync(_currentUserId);
        return Ok(sessions);
    }

    /// <summary>
    /// Retrieves a specific workout session by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the workout session to retrieve.</param>
    /// <returns>The requested workout session.</returns>
    /// <response code="200">Returns the requested workout session.</response>
    /// <response code="404">If the workout session with the specified ID is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType<Session>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Session>> GetWorkout(Guid id)
    {
        var session = await repository.GetByIdAsync(id);

        if (session == null)
            return NotFound("جلسه تمرینی مورد نظر پیدا نشد.");

        return Ok(session);
    }

    /// <summary>
    /// Deletes a specific workout session.
    /// </summary>
    /// <param name="id">The ID of the workout session to delete.</param>
    /// <returns>An empty response indicating success.</returns>
    /// <response code="204">Indicates the workout session was successfully deleted.</response>
    /// <response code="404">If the workout session with the specified ID is not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkout(Guid id)
    {
        var session = await repository.GetByIdAsync(id);
        if (session == null) return NotFound();

        repository.Delete(session);
        await repository.SaveChangesAsync();

        return NoContent();
    }
}