using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Services;

public class DailyWeightService(AppDbContext dbContext) : IDailyWeightService
{
    public async Task<DailyWeight> LogWeightAsync(DailyWeightRequest request)
    {
        var entry = new DailyWeight
        {
            Date = request.Date,
            Weight = request.Weight,
            Note = request.Note
        };

        dbContext.DailyWeights.Add(entry);
        await dbContext.SaveChangesAsync();

        return entry;
    }
}