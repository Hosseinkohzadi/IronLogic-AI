using IronLogic.Application.Interfaces;
using IronLogic.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers.Admin;

/// <summary>
/// Administrative controller for managing exercise approvals.
/// </summary>
/// <param name="adminService">The admin service.</param>
/// <param name="exerciseRepository">The exercise repository.</param>
[ApiController]
[Route("api/v1/admin/exercise-approvals")]
[Produces("application/json")]
public class ExerciseApprovalController(IAdminService adminService, IExerciseRepository exerciseRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves all exercises pending admin approval.
    /// </summary>
    /// <returns>A list of exercises awaiting approval.</returns>
    /// <response code="200">Returns the list of pending exercises.</response>
    [HttpGet("pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPendingApprovals()
    {
        var exercises = await exerciseRepository.GetPendingApprovalsAsync();
        return Ok(exercises);
    }

    /// <summary>
    /// Approves an exercise, making it globally visible.
    /// </summary>
    /// <param name="exerciseId">The unique identifier of the exercise to approve.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Exercise approved successfully.</response>
    /// <response code="404">Exercise not found.</response>
    [HttpPost("{exerciseId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ApproveExercise(Guid exerciseId)
    {
        var result = await adminService.ApproveExerciseAsync(exerciseId);
        
        if (!result)
            return NotFound(new { Message = "Exercise not found" });

        return Ok(new { Message = "Exercise approved successfully" });
    }

    /// <summary>
    /// Rejects an exercise submission.
    /// </summary>
    /// <param name="exerciseId">The unique identifier of the exercise to reject.</param>
    /// <param name="request">The rejection request containing optional reason.</param>
    /// <returns>Success or failure result.</returns>
    /// <response code="200">Exercise rejected successfully.</response>
    /// <response code="404">Exercise not found.</response>
    [HttpPost("{exerciseId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RejectExercise(Guid exerciseId, [FromBody] RejectExerciseRequest? request)
    {
        var result = await adminService.RejectExerciseAsync(exerciseId, request?.Reason);
        
        if (!result)
            return NotFound(new { Message = "Exercise not found" });

        return Ok(new { Message = "Exercise rejected successfully" });
    }
}

/// <summary>
/// Request model for rejecting an exercise.
/// </summary>
public record RejectExerciseRequest
{
    /// <summary>
    /// Gets or sets the reason for rejection.
    /// </summary>
    public string? Reason { get; set; }
}
