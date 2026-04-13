using System.ComponentModel.DataAnnotations;

using IronLogic.Domain.Enums;

namespace IronLogic.Application.DTOs.Profile;

/// <summary>
/// Request model for updating account profile and basic identity fields.
/// </summary>
public record UpdateProfileDto
{
    /// <summary>
    /// Gets or sets the user's email.
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string? Email { get; init; }

    /// <summary>
    /// Gets or sets the user's display name (username).
    /// </summary>
    [StringLength(100, ErrorMessage = "Name must be 100 characters or fewer")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    [StringLength(50, ErrorMessage = "First name must be 50 characters or fewer")]
    public string? FirstName { get; init; }

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    [StringLength(50, ErrorMessage = "Last name must be 50 characters or fewer")]
    public string? LastName { get; init; }

    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number must be 20 characters or fewer")]
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Gets or sets the user's profile picture URL.
    /// </summary>
    [Url(ErrorMessage = "Invalid URL format")]
    [StringLength(500, ErrorMessage = "Profile picture URL must be 500 characters or fewer")]
    public string? ProfilePictureUrl { get; init; }

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
    [Range(0, 300, ErrorMessage = "Height must be between 0 and 300 centimeters")]
    public decimal? Height { get; init; }

    /// <summary>
    /// Gets or sets the user's current weight in kilograms.
    /// </summary>
    [Range(0, 1000, ErrorMessage = "CurrentWeight must be between 0 and 1000 kilograms")]
    public decimal? CurrentWeight { get; init; }

    /// <summary>
    /// Gets or sets the user's target weight in kilograms.
    /// </summary>
    [Range(0, 1000, ErrorMessage = "TargetWeight must be between 0 and 1000 kilograms")]
    public decimal? TargetWeight { get; init; }

    /// <summary>
    /// Gets or sets the user's activity level.
    /// </summary>
    public ActivityLevel ActivityLevel { get; init; } = ActivityLevel.None;

    /// <summary>
    /// Gets or sets the user's biography.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Bio must be 1000 characters or fewer")]
    public string? Bio { get; init; }
}
