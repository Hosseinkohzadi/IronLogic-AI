namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents an application user.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// The unique identifier for the user.
    /// </summary>
    public new Guid Id { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// The user's email address, used for login and communication.
    /// </summary>
    public required string Email { get; set; } = "kohzadi_hossein@yahoo.com";

    /// <summary>
    /// The user's unique username.
    /// </summary>
    public required string Username { get; set; } = "kohzadi_hossein";

    /// <summary>
    /// The hashed password for the user's account.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// A collection of workout sessions recorded by the user.
    /// </summary>
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<DailyWeight> DailyWeights { get; set; } = new List<DailyWeight>();
}