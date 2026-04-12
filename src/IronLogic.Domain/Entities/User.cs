using IronLogic.Domain.Enums;

using Microsoft.AspNetCore.Identity;

namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a user in the IronLogic system, extending ASP.NET Core Identity.
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    /// Gets or sets the measurement unit system preference for this user (Metric or Imperial).
    /// Essential for international users in Canada, USA, and globally.
    /// </summary>
    public UnitSystem UnitSystem { get; set; } = UnitSystem.Metric;

    /// <summary>
    /// Gets or sets the preferred currency for financial transactions and subscriptions.
    /// Supports multi-currency: CAD, USD, EUR, GBP, AUD for global operations.
    /// </summary>
    public Currency PreferredCurrency { get; set; } = Currency.USD;

    /// <summary>
    /// Gets or sets the user's timezone identifier (IANA format, e.g., "America/Toronto", "Europe/London").
    /// Used to ensure all DateTime displays are localized to user's local time.
    /// </summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// Gets or sets the two-letter ISO country code (e.g., "CA", "US", "GB") for tax calculation purposes.
    /// Essential for Canadian GST/HST and international tax compliance.
    /// </summary>
    public string CountryCode { get; set; } = "US";

    /// <summary>
    /// Gets or sets the collection of workout sessions belonging to this user.
    /// </summary>
    public ICollection<Session> Sessions { get; set; } = new List<Session>();

    /// <summary>
    /// Gets or sets the collection of daily weight records belonging to this user.
    /// </summary>
    public ICollection<DailyWeight> DailyWeights { get; set; } = new List<DailyWeight>();

    /// <summary>
    /// Gets or sets the collection of user subscriptions belonging to this user.
    /// </summary>
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();

    /// <summary>
    /// Gets or sets the collection of payment transactions belonging to this user.
    /// </summary>
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    /// <summary>
    /// Gets or sets the collection of communication history records for this user.
    /// </summary>
    public ICollection<CommunicationHistory> CommunicationHistories { get; set; } = new List<CommunicationHistory>();

    /// <summary>
    /// Gets or sets the user's profile details.
    /// </summary>
    public UserProfile? Profile { get; set; }
}