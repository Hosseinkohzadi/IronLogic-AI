using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;

using Hangfire;

using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Handles automatic email communication workflows triggered by background jobs.
/// </summary>
public class EmailAutomationService(
    AppDbContext dbContext,
    IEmailService emailService,
  IBackgroundJobClient backgroundJobClient,
    ILogger<EmailAutomationService> logger) : IEmailAutomationService
{
    /// <inheritdoc />
    public async Task SendConfirmationCodeEmailAsync(
        string userId,
        string code,
        CancellationToken cancellationToken = default)
    {
        const string subject = "Your IronLogic AI Verification Code";
        var body = BuildConfirmationCodeBody(code);
        await emailService.SendAndLogEmailAsync(userId, subject, body, isManual: false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendWelcomeEmailAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string subject = "Welcome to IronLogic AI — You're In!";
        var body = BuildWelcomeEmailBody();
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
            return;

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
            const string body = "<p>Your subscription will expire in 3 days. Renew now to keep your progress uninterrupted.</p>";
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
            return;

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
            const string body = "<p>We have not seen your workout activity in the last 2 days. Jump back in and keep your streak alive.</p>";
            await ExecuteEmailSafelyAsync(userId, subject, body, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task QueueDiscountOfferBroadcastAsync(
      string subject,
      decimal discountPercentage,
      string? customMessage,
      string callToActionUrl,
      CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(callToActionUrl);

        var userIds = await dbContext.Users
          .AsNoTracking()
          .Where(u => !string.IsNullOrWhiteSpace(u.Email))
          .Select(u => u.Id)
          .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            backgroundJobClient.Enqueue<IEmailAutomationService>(service =>
              service.SendDiscountOfferEmailAsync(
                userId,
                subject,
                discountPercentage,
                customMessage,
                callToActionUrl,
                CancellationToken.None));
        }

        logger.LogInformation(
          "Queued discount campaign for {UserCount} users. Discount: {DiscountPercentage}",
          userIds.Count,
          discountPercentage);
    }

    /// <inheritdoc />
    public async Task SendDiscountOfferEmailAsync(
      string userId,
      string subject,
      decimal discountPercentage,
      string? customMessage,
      string callToActionUrl,
      CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(callToActionUrl);

        var user = await dbContext.Users
          .AsNoTracking()
          .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("Discount email skipped. User {UserId} not found.", userId);
            return;
        }

        var model = new
        {
            Subject = subject,
            DisplayName = string.IsNullOrWhiteSpace(user.UserName) ? "Athlete" : user.UserName,
            DiscountPercentage = discountPercentage,
            CustomMessage = string.IsNullOrWhiteSpace(customMessage)
            ? "Upgrade today and keep progressing without interruption."
            : customMessage,
            CallToActionUrl = callToActionUrl
        };

        await emailService.SendAndLogTemplatedEmailAsync(
          userId,
          subject,
          templateName: "DiscountOffer",
          model,
          isManual: false,
          cancellationToken);
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

    private static string BuildConfirmationCodeBody(string code) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <body style="margin:0;padding:40px;background-color:#f4f6f9;font-family:Arial,sans-serif;">
          <div style="max-width:480px;margin:0 auto;background:#ffffff;border-radius:10px;padding:48px 40px;box-shadow:0 4px 16px rgba(0,0,0,0.08);">
            <h1 style="margin:0 0 8px;font-size:24px;color:#1a1a2e;">Verify Your Email Address</h1>
            <p style="color:#555555;line-height:1.6;margin-bottom:32px;">
              Enter the code below in the IronLogic AI app to confirm your account.
              This code expires in <strong>10 minutes</strong>.
            </p>
            <div style="text-align:center;background:#f0f4ff;border-radius:8px;padding:24px 0;margin-bottom:32px;">
              <span style="font-size:52px;font-weight:bold;letter-spacing:14px;color:#e94560;font-family:monospace;">{code}</span>
            </div>
            <p style="color:#999999;font-size:13px;">
              If you did not create an IronLogic AI account, you can safely ignore this email.
            </p>
          </div>
        </body>
        </html>
        """;

    private static string BuildWelcomeEmailBody() => """
        <!DOCTYPE html>
        <html lang="en">
        <body style="margin:0;padding:40px;background-color:#0f3460;font-family:Arial,sans-serif;">
          <div style="max-width:560px;margin:0 auto;background:#16213e;border-radius:12px;padding:48px 40px;color:#ffffff;">
            <div style="text-align:center;margin-bottom:40px;">
              <h1 style="margin:0;font-size:36px;color:#e94560;letter-spacing:2px;">IronLogic AI</h1>
              <p style="margin:8px 0 0;color:#8892b0;font-size:14px;">Your AI-Powered Training Partner</p>
            </div>
            <h2 style="font-size:26px;color:#ffffff;margin-bottom:16px;">Welcome, Champion! &#127942;</h2>
            <p style="color:#c0cce0;line-height:1.8;margin-bottom:12px;">
              Your email is verified and your account is fully activated.
            </p>
            <p style="color:#c0cce0;line-height:1.8;margin-bottom:32px;">
              Start logging workouts, tracking your progress, and let our AI engine build the optimal training plan for you.
            </p>
            <div style="text-align:center;margin:40px 0;">
              <a href="https://app.ironlogic.ai"
                 style="display:inline-block;background:#e94560;color:#ffffff;padding:16px 48px;border-radius:8px;text-decoration:none;font-size:18px;font-weight:bold;letter-spacing:0.5px;">
                Get Started &#8594;
              </a>
            </div>
            <hr style="border:none;border-top:1px solid #2a3055;margin:32px 0;">
            <p style="color:#4a5568;font-size:12px;text-align:center;margin:0;">
              &copy; 2026 IronLogic AI. All rights reserved.
            </p>
          </div>
        </body>
        </html>
        """;
}
