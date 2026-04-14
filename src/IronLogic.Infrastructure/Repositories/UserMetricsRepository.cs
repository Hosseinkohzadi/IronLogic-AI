using IronLogic.Domain.Interfaces;
using IronLogic.Domain.Models;
using IronLogic.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Repositories;

/// <summary>
/// Implements user metrics and analytics queries.
/// </summary>
/// <param name="context">The database context.</param>
public class UserMetricsRepository(AppDbContext context) : IUserMetricsRepository
{
    /// <inheritdoc />
    public async Task<UserMetrics> GetUserMetricsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var oneWeekAgo = now.AddDays(-7);
        var twoWeeksAgo = now.AddDays(-14);

        var currentPremiumCount = await context.UserSubscriptions
            .Where(s => s.IsActive && s.EndDate >= now)
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var previousPremiumCount = await context.UserSubscriptions
            .Where(s => s.IsActive && s.EndDate >= oneWeekAgo && s.EndDate < now)
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var currentWeeklyActiveUsers = await context.Sessions
            .Where(s => s.Date >= oneWeekAgo)
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var previousWeeklyActiveUsers = await context.Sessions
            .Where(s => s.Date >= twoWeeksAgo && s.Date < oneWeekAgo)
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalSessions = await context.Sessions
            .CountAsync(cancellationToken);

        var currentWeekSessions = await context.Sessions
            .CountAsync(s => s.Date >= oneWeekAgo, cancellationToken);

        var previousWeekSessions = await context.Sessions
            .CountAsync(s => s.Date >= twoWeeksAgo && s.Date < oneWeekAgo, cancellationToken);

        var usersWithNoActivityInThreeWeeks = await context.Users
            .Where(u => !context.Sessions.Any(s => s.UserId == u.Id && s.Date >= now.AddDays(-21)))
            .CountAsync(cancellationToken);

        var usersWithNoActivityInFourWeeks = await context.Users
            .Where(u => !context.Sessions.Any(s => s.UserId == u.Id && s.Date >= now.AddDays(-28)))
            .CountAsync(cancellationToken);

        var premiumTrend = CalculateTrend(currentPremiumCount, previousPremiumCount);
        var wauTrend = CalculateTrend(currentWeeklyActiveUsers, previousWeeklyActiveUsers);
        var sessionsTrend = CalculateTrend(currentWeekSessions, previousWeekSessions);
        var churnTrend = CalculateTrend(usersWithNoActivityInThreeWeeks, usersWithNoActivityInFourWeeks);

        return new UserMetrics
        {
            PremiumSubscribers = currentPremiumCount,
            WeeklyActiveUsers = currentWeeklyActiveUsers,
            TotalSessions = totalSessions,
            ChurnRiskCount = usersWithNoActivityInThreeWeeks,
            PremiumTrend = premiumTrend,
            WauTrend = wauTrend,
            SessionsTrend = sessionsTrend,
            ChurnTrend = churnTrend
        };
    }

    private static decimal CalculateTrend(int current, int previous)
    {
        if (previous == 0)
            return current > 0 ? 100m : 0m;

        return Math.Round(((current - previous) / (decimal)previous) * 100, 2);
    }
}

