namespace IronLogic.Application.DTOs.Settings;

/// <summary>
/// Represents a platform setting in API responses.
/// </summary>
public record PlatformSettingDto
{
    /// <summary>
    /// Gets the setting key.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets the setting value.
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Gets the setting description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the UTC timestamp indicating when this setting was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
