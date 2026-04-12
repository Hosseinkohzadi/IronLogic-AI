using IronLogic.Domain.Enums;

namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a persisted record of an email communication attempt for a user.
/// </summary>
public class CommunicationHistory : BaseEntity
{
    /// <summary>
    /// Gets or sets the target user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the related user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email body in HTML format.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC date and time when sending was attempted.
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// Gets or sets the communication delivery status.
    /// </summary>
    public CommunicationStatus Status { get; set; } = CommunicationStatus.Unknown;

    /// <summary>
    /// Gets or sets the communication trigger type.
    /// </summary>
    public CommunicationType Type { get; set; } = CommunicationType.Unknown;
}
