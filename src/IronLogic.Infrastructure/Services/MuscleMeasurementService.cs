using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Infrastructure.Data;

namespace IronLogic.Infrastructure.Services;

public class MuscleMeasurementService(AppDbContext dbContext) : IMuscleMeasurementService
{
    public async Task<MuscleMeasurement> LogMeasurementAsync(MuscleMeasurementRequest request)
    {
        var entry = new MuscleMeasurement
        {
            Date = request.Date,
            Neck = request.Neck,
            Chest = request.Chest,
            Waist = request.Waist,
            BicepsLeft = request.BicepsLeft,
            BicepsRight = request.BicepsRight,
            ThighLeft = request.ThighLeft,
            ThighRight = request.ThighRight
        };

        dbContext.MuscleMeasurements.Add(entry);
        await dbContext.SaveChangesAsync();

        return entry;
    }
}
