namespace IronLogic.Application.DTOs.User;

/// <summary>
/// Data transfer object for user list in admin grid
/// </summary>
public record AdminUserListDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the user
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's primary role
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's subscription plan
    /// </summary>
    public string Plan { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription status
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription end date
    /// </summary>
    public DateTimeOffset? SubscriptionEndDate { get; init; }

    /// <summary>
    /// Gets or sets the profile image URL
    /// </summary>
    public string ProfileImageUrl { get; init; } = string.Empty;
}
