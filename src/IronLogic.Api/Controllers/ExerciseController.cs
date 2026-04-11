using IronLogic.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Controller for managing user-accessible exercises.
/// </summary>
/// <param name="exerciseRepository">The exercise repository.</param>
[ApiController]
[Route("api/v1/exercises")]
[Produces("application/json")]
public class ExerciseController(IExerciseRepository exerciseRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves all exercises available to the current user.
    /// Includes globally approved exercises and user's private exercises.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list of available exercises.</returns>
    /// <response code="200">Returns the list of available exercises.</response>
    [HttpGet("available")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAvailableExercises([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { Message = "User ID is required" });

        var exercises = await exerciseRepository.GetAvailableExercisesAsync(userId);
        return Ok(exercises);
    }

    /// <summary>
    /// Retrieves exercises created by a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list of exercises created by the user.</returns>
    /// <response code="200">Returns the list of user's exercises.</response>
    [HttpGet("my-exercises")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMyExercises([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { Message = "User ID is required" });

        var exercises = await exerciseRepository.GetExercisesByCreatorAsync(userId);
        return Ok(exercises);
    }
}
