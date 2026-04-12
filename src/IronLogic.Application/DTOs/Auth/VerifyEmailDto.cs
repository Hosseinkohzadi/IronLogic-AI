using System.ComponentModel.DataAnnotations;

namespace IronLogic.Application.DTOs.Auth;

/// <summary>
/// Payload for the email verification endpoint.
/// </summary>
public record VerifyEmailDto
{
    /// <summary>
    /// Gets the identifier of the user being verified.
    /// </summary>
    [Required]
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the six-digit verification code sent to the user's email.
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be exactly 6 digits.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must consist of 6 numeric digits.")]
    public string Code { get; init; } = string.Empty;
}
