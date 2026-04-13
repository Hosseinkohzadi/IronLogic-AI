namespace IronLogic.Application.Interfaces;

/// <summary>
/// Provides email communication operations with delivery history logging.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to a user and persists its delivery history.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body as HTML.</param>
    /// <param name="isManual">Whether the message is manually triggered.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAndLogEmailAsync(
        string userId,
        string subject,
        string body,
        bool isManual,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders an HTML template and sends the email while persisting delivery history.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="templateName">Template file name without extension.</param>
    /// <param name="model">Template model values.</param>
    /// <param name="isManual">Whether the message is manually triggered.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAndLogTemplatedEmailAsync(
        string userId,
        string subject,
        string templateName,
        object model,
        bool isManual,
        CancellationToken cancellationToken = default);
}
