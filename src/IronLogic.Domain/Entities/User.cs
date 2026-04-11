using Microsoft.AspNetCore.Identity;

namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a user in the IronLogic system, extending ASP.NET Core Identity.
/// </summary>
public class User : IdentityUser
{
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
}