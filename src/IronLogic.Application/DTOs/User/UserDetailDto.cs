namespace IronLogic.Application.DTOs.User;

/// <summary>
/// Data transfer object containing detailed user information including claims, roles, and lockout status
/// </summary>
public record UserDetailDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the user
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's username
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    /// Gets or sets the user's email address
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets or sets whether the email has been confirmed
    /// </summary>
    public bool EmailConfirmed { get; init; }

    /// <summary>
    /// Gets or sets the user's phone number
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Gets or sets whether the phone number has been confirmed
    /// </summary>
    public bool PhoneNumberConfirmed { get; init; }

    /// <summary>
    /// Gets or sets whether two-factor authentication is enabled
    /// </summary>
    public bool TwoFactorEnabled { get; init; }

    /// <summary>
    /// Gets or sets the lockout end date
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; init; }

    /// <summary>
    /// Gets or sets whether lockout is enabled for this user
    /// </summary>
    public bool LockoutEnabled { get; init; }

    /// <summary>
    /// Gets or sets the number of failed access attempts
    /// </summary>
    public int AccessFailedCount { get; init; }

    /// <summary>
    /// Gets or sets the list of roles assigned to the user
    /// </summary>
    public IReadOnlyList<string> Roles { get; init; } = new List<string>();

    /// <summary>
    /// Gets or sets the list of claims assigned to the user
    /// </summary>
    public IReadOnlyList<UserClaimDto> Claims { get; init; } = new List<UserClaimDto>();
}

/// <summary>
/// Represents a user claim
/// </summary>
public record UserClaimDto
{
    /// <summary>
    /// Gets or sets the claim type
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the claim value
    /// </summary>
    public string Value { get; init; } = string.Empty;
}
