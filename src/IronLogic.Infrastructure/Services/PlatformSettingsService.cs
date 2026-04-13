using System.Globalization;

using IronLogic.Application.DTOs.Settings;
using IronLogic.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Handles retrieval and updates of platform-level settings.
/// </summary>
public class PlatformSettingsService(AppDbContext dbContext) : IPlatformSettingsService
{
    private const string YearlyDiscountPercentageKey = "YearlyDiscountPercentage";

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlatformSettingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.PlatformSettings
            .OrderBy(s => s.Key)
            .Select(s => new PlatformSettingDto
            {
                Key = s.Key,
                Value = s.Value,
                Description = s.Description,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlatformSettingDto?> UpdateValueAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var setting = await dbContext.PlatformSettings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

        if (setting is null)
            return null;

        setting.Value = value;
        setting.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new PlatformSettingDto
        {
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            UpdatedAt = setting.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<PricingConfigDto> GetPublicPricingConfigAsync(CancellationToken cancellationToken = default)
    {
        var yearlyDiscountSetting = await dbContext.PlatformSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == YearlyDiscountPercentageKey, cancellationToken);

        if (yearlyDiscountSetting is null)
            return new PricingConfigDto { YearlyDiscountPercentage = 0 };

        var parsed = decimal.TryParse(
            yearlyDiscountSetting.Value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var discount);

        return new PricingConfigDto
        {
            YearlyDiscountPercentage = parsed ? discount : 0
        };
    }
}
