using IronLogic.Application.DTOs.Settings;

namespace IronLogic.Application.Interfaces;

/// <summary>
/// Provides operations for managing platform-level configuration settings.
/// </summary>
public interface IPlatformSettingsService
{
    /// <summary>
    /// Retrieves all platform settings.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All platform settings.</returns>
    Task<IReadOnlyList<PlatformSettingDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the value of a platform setting identified by key.
    /// </summary>
    /// <param name="key">The unique setting key.</param>
    /// <param name="value">The new setting value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated setting, or <c>null</c> if no setting exists for the provided key.</returns>
    Task<PlatformSettingDto?> UpdateValueAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the safe, public pricing configuration consumed by athlete clients.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Public pricing configuration values.</returns>
    Task<PricingConfigDto> GetPublicPricingConfigAsync(CancellationToken cancellationToken = default);
}
