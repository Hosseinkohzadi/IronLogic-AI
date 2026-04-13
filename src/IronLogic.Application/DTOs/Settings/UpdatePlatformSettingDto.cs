using System.ComponentModel.DataAnnotations;

namespace IronLogic.Application.DTOs.Settings;

/// <summary>
/// Payload for updating a platform setting value.
/// </summary>
public record UpdatePlatformSettingDto
{
    /// <summary>
    /// Gets the new value for the setting.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Value { get; init; } = string.Empty;
}
