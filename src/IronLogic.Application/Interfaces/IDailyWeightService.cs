using IronLogic.Application.DTOs;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

public interface IDailyWeightService
{
    Task<DailyWeight> LogWeightAsync(DailyWeightRequest request);
}