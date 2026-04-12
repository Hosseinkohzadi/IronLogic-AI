namespace IronLogic.Application.Interfaces;

/// <summary>
/// Provides automatic email workflows used by background jobs.
/// </summary>
public interface IEmailAutomationService
{
    /// <summary>
    /// Sends a verification code email to a newly registered user using an HTML template
    /// that displays the six-digit code prominently.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="code">The six-digit OTP code to embed in the email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendConfirmationCodeEmailAsync(string userId, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a premium welcome email to a user whose email has just been verified.
    /// Includes a "Get Started" call-to-action button.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendWelcomeEmailAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends subscription expiry warning emails for users whose subscriptions expire in three days.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendSubscriptionExpiryWarningsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends workout reminders to users with no workout activity in the last two days.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendWorkoutRemindersAsync(CancellationToken cancellationToken = default);
}

