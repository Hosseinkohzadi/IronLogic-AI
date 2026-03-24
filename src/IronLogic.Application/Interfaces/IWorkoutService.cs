using IronLogic.Application.DTOs;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

public interface IWorkoutService
{
    Task<List<WorkoutSession>> GetSessionsAsync();
    Task<WorkoutStatsResponse> GetStatsAsync();
}