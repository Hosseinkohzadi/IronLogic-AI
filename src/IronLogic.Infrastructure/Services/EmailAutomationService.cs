using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;

using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Handles automatic email communication workflows triggered by background jobs.
/// </summary>
public class EmailAutomationService(
    AppDbContext dbContext,
    IEmailService emailService,
    ILogger<EmailAutomationService> logger) : IEmailAutomationService
{
    /// <inheritdoc />
    public async Task SendWelcomeEmailAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string subject = "Welcome to IronLogic AI";
        const string body = "<h2>Welcome to IronLogic AI</h2><p>Your account is now active. Let's start building your training momentum.</p>";

        await emailService.SendAndLogEmailAsync(userId, subject, body, isManual: false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendSubscriptionExpiryWarningsAsync(CancellationToken cancellationToken = default)
    {
        var targetDate = DateTime.UtcNow.Date.AddDays(3);
        var today = DateTime.UtcNow.Date;

        var userIds = await dbContext.UserSubscriptions
            .Where(us => us.IsActive && us.EndDate.Date == targetDate)
            .Select(us => us.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (userIds.Count == 0)
        {
            return;
        }

        const string subject = "Subscription Expiry Warning";

        var alreadyNotified = await dbContext.CommunicationHistories
            .Where(ch => ch.Type == CommunicationType.Automatic
                         && ch.Subject == subject
                         && ch.SentAt.Date == today
                         && userIds.Contains(ch.UserId)
                         && ch.Status == CommunicationStatus.Sent)
            .Select(ch => ch.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var pendingUserIds = userIds.Except(alreadyNotified, StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var userId in pendingUserIds)
        {
            var body = "<p>Your subscription will expire in 3 days. Renew now to keep your progress uninterrupted.</p>";
            await ExecuteEmailSafelyAsync(userId, subject, body, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task SendWorkoutRemindersAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-2);
        var today = DateTime.UtcNow.Date;

        var activeUserIds = await dbContext.Sessions
            .Where(s => s.Date >= cutoff)
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var targetUserIds = await dbContext.Users
            .Where(u => !activeUserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (targetUserIds.Count == 0)
        {
            return;
        }

        const string subject = "Workout Reminder";

        var alreadyNotified = await dbContext.CommunicationHistories
            .Where(ch => ch.Type == CommunicationType.Automatic
                         && ch.Subject == subject
                         && ch.SentAt.Date == today
                         && targetUserIds.Contains(ch.UserId)
                         && ch.Status == CommunicationStatus.Sent)
            .Select(ch => ch.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var pendingUserIds = targetUserIds.Except(alreadyNotified, StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var userId in pendingUserIds)
        {
            var body = "<p>We have not seen your workout activity in the last 2 days. Jump back in and keep your streak alive.</p>";
            await ExecuteEmailSafelyAsync(userId, subject, body, cancellationToken);
        }
    }

    private async Task ExecuteEmailSafelyAsync(
        string userId,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            await emailService.SendAndLogEmailAsync(userId, subject, body, isManual: false, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Automatic email dispatch failed for user {UserId}", userId);
        }
    }
}
