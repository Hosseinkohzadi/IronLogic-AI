namespace IronLogic.Application.DTOs.User;

/// <summary>
/// Data transfer object for admin user metrics dashboard cards
/// </summary>
public record AdminUserMetricsDto
{
    /// <summary>
    /// Gets or sets the total count of premium subscribers
    /// </summary>
    public int PremiumSubscribers { get; init; }

    /// <summary>
    /// Gets or sets the count of weekly active users
    /// </summary>
    public int WeeklyActiveUsers { get; init; }

    /// <summary>
    /// Gets or sets the total count of workout sessions
    /// </summary>
    public int TotalSessions { get; init; }

    /// <summary>
    /// Gets or sets the count of users at churn risk
    /// </summary>
    public int ChurnRiskCount { get; init; }

    /// <summary>
    /// Gets or sets the percentage trend for premium subscribers
    /// </summary>
    public decimal PremiumTrend { get; init; }

    /// <summary>
    /// Gets or sets the percentage trend for weekly active users
    /// </summary>
    public decimal WauTrend { get; init; }

    /// <summary>
    /// Gets or sets the percentage trend for total sessions
    /// </summary>
    public decimal SessionsTrend { get; init; }

    /// <summary>
    /// Gets or sets the percentage trend for churn risk count
    /// </summary>
    public decimal ChurnTrend { get; init; }
}
