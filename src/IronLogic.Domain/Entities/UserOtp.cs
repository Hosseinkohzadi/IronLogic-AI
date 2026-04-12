namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a one-time password issued to a user for email verification.
/// </summary>
public class UserOtp : BaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user this OTP belongs to.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the navigation property for the associated user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the six-digit numeric code shown to the user.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ASP.NET Core Identity email-confirmation token linked to this OTP.
    /// Used to call <c>UserManager.ConfirmEmailAsync</c> once the code is validated.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC time at which this OTP expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this OTP has already been consumed.
    /// </summary>
    public bool IsUsed { get; set; }
}
