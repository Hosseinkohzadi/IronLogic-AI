using System.ComponentModel.DataAnnotations;

namespace IronLogic.Application.DTOs.Communication;

/// <summary>
/// Request payload for manually sending a custom email to a user.
/// </summary>
public record SendEmailRequestDto
{
    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    [Required]
    [StringLength(200, ErrorMessage = "Subject must be 200 characters or fewer")]
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the email body as HTML.
    /// </summary>
    [Required]
    [StringLength(20000, ErrorMessage = "Body must be 20000 characters or fewer")]
    public string Body { get; init; } = string.Empty;
}
