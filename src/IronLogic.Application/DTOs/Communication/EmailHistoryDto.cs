namespace IronLogic.Application.DTOs.Communication;

/// <summary>
/// Data transfer object for email history records
/// </summary>
public record EmailHistoryDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the email record
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the email subject
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time the email was sent
    /// </summary>
    public string SentAt { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the delivery status of the email
    /// </summary>
    public string Status { get; init; } = string.Empty;
}
