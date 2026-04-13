using IronLogic.Application.DTOs.Communication;
using IronLogic.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Controller for managing user communications and email history
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class CommunicationsController(
    ICommunicationService communicationService,
    ILogger<CommunicationsController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves the email communication history for a specific user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of email communications sent to the user</returns>
    /// <response code="200">Returns the list of email communications</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{userId}/emails")]
    [ProducesResponseType<IReadOnlyList<EmailHistoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserEmailHistory(
        string userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving email history for user: {UserId}", userId);

        var emailHistory = await communicationService.GetUserEmailHistoryAsync(userId, cancellationToken);

        logger.LogInformation("Retrieved {Count} email records for user: {UserId}", emailHistory.Count, userId);
        return Ok(emailHistory);
    }
}
