using IronLogic.Domain.Enums;

namespace IronLogic.Application.DTOs.Profile;

/// <summary>
/// Represents account identity and profile data for the current user.
/// </summary>
public record UserProfileResponseDto
{
    /// <summary>
    /// Gets or sets the unique user identifier.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets or sets the user's display name (username).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's profile picture URL.
    /// </summary>
    public string ProfilePictureUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's gender.
    /// </summary>
    public Gender Gender { get; init; } = Gender.Unknown;

    /// <summary>
    /// Gets or sets the user's date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; init; }

    /// <summary>
    /// Gets or sets the user's height in centimeters.
    /// </summary>
    public decimal? Height { get; init; }

    /// <summary>
    /// Gets or sets the user's current weight in kilograms.
    /// </summary>
    public decimal? CurrentWeight { get; init; }

    /// <summary>
    /// Gets or sets the user's target weight in kilograms.
    /// </summary>
    public decimal? TargetWeight { get; init; }

    /// <summary>
    /// Gets or sets the user's activity level.
    /// </summary>
    public ActivityLevel ActivityLevel { get; init; } = ActivityLevel.None;

    /// <summary>
    /// Gets or sets the user's biography.
    /// </summary>
    public string? Bio { get; init; }
}
