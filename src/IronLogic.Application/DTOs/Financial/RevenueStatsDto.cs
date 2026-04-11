namespace IronLogic.Application.DTOs.Financial;

/// <summary>
/// DTO for aggregated revenue and subscription statistics.
/// Used by the Angular Financial Dashboard with camelCase serialization.
/// </summary>
public class RevenueStatsDto
{
    /// <summary>
    /// Gets or sets the total monthly revenue in the base currency.
    /// </summary>
    public decimal monthlyRevenue { get; set; }

    /// <summary>
    /// Gets or sets the total yearly revenue (extrapolated or actual).
    /// </summary>
    public decimal yearlyRevenue { get; set; }

    /// <summary>
    /// Gets or sets the number of active subscriptions.
    /// </summary>
    public int activeSubscriptions { get; set; }

    /// <summary>
    /// Gets or sets the number of pending payment transactions.
    /// </summary>
    public int pendingPayments { get; set; }

    /// <summary>
    /// Gets or sets the churn rate as a percentage (e.g., 4.8 for 4.8%).
    /// </summary>
    public decimal churnRate { get; set; }

    /// <summary>
    /// Gets or sets the revenue growth percentage compared to the previous month.
    /// </summary>
    public decimal revenueGrowth { get; set; }

    /// <summary>
    /// Gets or sets the base currency code for the stats (USD, CAD, EUR).
    /// </summary>
    public string baseCurrency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the monthly revenue data points for the chart.
    /// </summary>
    public List<MonthlyRevenueDto> monthlyRevenueData { get; set; } = new();
}

/// <summary>
/// DTO for monthly revenue data points.
/// </summary>
public class MonthlyRevenueDto
{
    /// <summary>
    /// Gets or sets the month label (e.g., "Jan", "Feb", "Mar").
    /// </summary>
    public string month { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the revenue amount for the month.
    /// </summary>
    public decimal amount { get; set; }
}
