namespace IronLogic.Domain.Enums;

/// <summary>
/// Represents the origin of an email communication.
/// </summary>
public enum CommunicationType
{
    /// <summary>
    /// The type is not defined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The email was triggered manually by an administrator.
    /// </summary>
    Manual = 1,

    /// <summary>
    /// The email was triggered automatically by the system.
    /// </summary>
    Automatic = 2
}
