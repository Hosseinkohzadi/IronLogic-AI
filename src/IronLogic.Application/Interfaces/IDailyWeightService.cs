using IronLogic.Application.DTOs;

namespace IronLogic.Application.Interfaces;

public interface IDailyWeightService
{
    Task<DailyWeight> LogWeightAsync(DailyWeightRequest request);
}