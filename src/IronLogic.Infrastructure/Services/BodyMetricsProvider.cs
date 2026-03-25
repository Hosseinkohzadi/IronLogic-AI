using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Services;

/// <summary>
///     EF Core-backed provider that retrieves the latest muscle measurement from the database.
/// </summary>
public class BodyMetricsProvider(AppDbContext dbContext) : IBodyMetricsProvider
{
    /// <inheritdoc />
    public async Task<MuscleMeasurement?> GetLatestMeasurementAsync()
    {
        return await dbContext.MuscleMeasurements
            .OrderByDescending(m => m.Date)
            .FirstOrDefaultAsync();
    }
}