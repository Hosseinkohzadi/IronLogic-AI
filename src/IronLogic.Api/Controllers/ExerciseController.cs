using IronLogic.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Controller for managing user-accessible exercises.
/// </summary>
/// <param name="exerciseService">The exercise service.</param>
[ApiController]
[Route("api/v1/exercises")]
[Produces("application/json")]
public class ExerciseController(IExerciseService exerciseService) : ControllerBase
{
    /// <summary>
    /// Retrieves all exercises available to the current user.
    /// Includes approved exercises and user's private exercises with ImageUrl support.
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

        var exercises = await exerciseService.GetAvailableExercisesAsync(userId);
        return Ok(exercises);
    }

    /// <summary>
    /// Retrieves exercises created by a specific user, including ImageUrl.
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

        var exercises = await exerciseService.GetExercisesByCreatorAsync(userId);
        return Ok(exercises);
    }
}
