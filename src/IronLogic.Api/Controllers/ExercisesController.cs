using IronLogic.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
//[Authorize] 
public class ExercisesController(IWorkoutService workoutService) : ControllerBase
{
    [HttpGet("{exerciseName}/history")]
    public async Task<IActionResult> GetHistory(string exerciseName)
    {
        var userId = "00000000-0000-0000-0000-000000000001";

        var result = await workoutService.GetExerciseHistoryAsync(userId, exerciseName);

        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }
}