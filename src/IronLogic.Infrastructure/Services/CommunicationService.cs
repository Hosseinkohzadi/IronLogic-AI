using IronLogic.Application.DTOs.Communication;
using IronLogic.Application.Interfaces;
using IronLogic.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Provides operations for managing user communications and email history
/// </summary>
public class CommunicationService(
    AppDbContext dbContext,
    ILogger<CommunicationService> logger) : ICommunicationService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<EmailHistoryDto>> GetUserEmailHistoryAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        logger.LogInformation("Fetching email history for user: {UserId}", userId);

        var communications = await dbContext.CommunicationHistories
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.SentAt)
            .Select(c => new EmailHistoryDto
            {
                Id = c.Id.ToString(),
                Subject = c.Subject,
                SentAt = c.SentAt.ToString("o"),
                Status = c.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} email records for user: {UserId}", communications.Count, userId);

        return communications;
    }
}
