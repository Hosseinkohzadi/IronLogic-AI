using IronLogic.Domain.Models;

namespace IronLogic.Domain.Interfaces;

/// <summary>
/// Defines operations for retrieving user metrics and analytics.
/// </summary>
public interface IUserMetricsRepository
{
    /// <summary>
    /// Retrieves administrative metrics for the user management dashboard.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User metrics including premium subscribers, active users, sessions, and churn risk.</returns>
    Task<UserMetrics> GetUserMetricsAsync(CancellationToken cancellationToken);
}

