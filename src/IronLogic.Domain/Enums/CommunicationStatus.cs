namespace IronLogic.Domain.Enums;

/// <summary>
/// Represents the delivery status of an email communication.
/// </summary>
public enum CommunicationStatus
{
    /// <summary>
    /// The status is not defined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The email was sent successfully.
    /// </summary>
    Sent = 1,

    /// <summary>
    /// The email delivery failed.
    /// </summary>
    Failed = 2
}
