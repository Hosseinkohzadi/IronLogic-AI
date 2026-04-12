using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;
using IronLogic.Infrastructure.Options;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Sends transactional emails and records their delivery result in communication history.
/// </summary>
public class EmailService(
    AppDbContext dbContext,
    UserManager<User> userManager,
    IOptions<EmailOptions> emailOptions,
    ILogger<EmailService> logger) : IEmailService
{
    /// <inheritdoc />
    public async Task SendAndLogEmailAsync(
        string userId,
        string subject,
        string body,
        bool isManual,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User '{userId}' was not found.");
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new InvalidOperationException($"User '{userId}' does not have a valid email.");
        }

        var options = emailOptions.Value;
        if (string.IsNullOrWhiteSpace(options.ApiKey) || string.IsNullOrWhiteSpace(options.FromEmail))
        {
            throw new InvalidOperationException("Email provider is not configured correctly.");
        }

        var history = new CommunicationHistory
        {
            UserId = userId,
            Subject = subject,
            Body = body,
            SentAt = DateTime.UtcNow,
            Type = isManual ? CommunicationType.Manual : CommunicationType.Automatic,
            Status = CommunicationStatus.Unknown
        };

        try
        {
            var client = new SendGridClient(options.ApiKey);
            var from = new EmailAddress(options.FromEmail, options.FromName);
            var to = new EmailAddress(user.Email, user.UserName ?? user.Email);

            var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: body);
            var response = await client.SendEmailAsync(message, cancellationToken);

            history.Status = response.IsSuccessStatusCode ? CommunicationStatus.Sent : CommunicationStatus.Failed;
            dbContext.CommunicationHistories.Add(history);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Email delivery failed for user {UserId}. StatusCode: {StatusCode}",
                    userId,
                    response.StatusCode);
                throw new InvalidOperationException("Email server is unavailable or rejected the request.");
            }

            logger.LogInformation("Email sent successfully for user {UserId}. Subject: {Subject}", userId, subject);
        }
        catch (HttpRequestException ex)
        {
            history.Status = CommunicationStatus.Failed;
            dbContext.CommunicationHistories.Add(history);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogError(ex, "Email send operation failed for user {UserId}", userId);
            throw new InvalidOperationException("Email server is unavailable. Please try again later.", ex);
        }
        catch (TaskCanceledException ex)
        {
            history.Status = CommunicationStatus.Failed;
            dbContext.CommunicationHistories.Add(history);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogError(ex, "Email send operation failed for user {UserId}", userId);
            throw new InvalidOperationException("Email server is unavailable. Please try again later.", ex);
        }
    }
}
