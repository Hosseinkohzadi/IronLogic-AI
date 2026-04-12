namespace IronLogic.Application.Interfaces;

/// <summary>
/// Provides automatic email workflows used by background jobs.
/// </summary>
public interface IEmailAutomationService
{
    /// <summary>
    /// Sends a welcome email to a newly registered user.
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
