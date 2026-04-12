using IronLogic.Domain.Enums;

namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents profile details for a user.
/// </summary>
public class UserProfile : BaseEntity
{
    /// <summary>
    /// Gets or sets the associated user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the associated user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the user biography.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Gets or sets the user's date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the user's height in centimeters.
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Gets or sets the user's weight in kilograms.
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Gets or sets the profile picture URL.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Gets or sets the user's fitness goal.
    /// </summary>
    public FitnessGoal FitnessGoal { get; set; } = FitnessGoal.None;
}