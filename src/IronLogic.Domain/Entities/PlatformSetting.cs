namespace IronLogic.Domain.Entities;

/// <summary>
/// Represents a configurable platform-level setting managed by SuperAdmins.
/// </summary>
public class PlatformSetting : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique setting key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the setting value as a string.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of the setting.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the last update.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
