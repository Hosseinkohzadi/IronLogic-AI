using IronLogic.Application.DTOs.Communication;

namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines operations for managing user communications and email history
/// </summary>
public interface ICommunicationService
{
    /// <summary>
    /// Retrieves the email communication history for a specific user
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of email history records.</returns>
    Task<IReadOnlyList<EmailHistoryDto>> GetUserEmailHistoryAsync(string userId, CancellationToken cancellationToken);
}
