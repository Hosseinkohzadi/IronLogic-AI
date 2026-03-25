using IronLogic.Application.DTOs;

namespace IronLogic.Application.Interfaces;

public interface IWorkoutService
{
    Task<List<WorkoutSession>> GetSessionsAsync();
    Task<WorkoutStatsResponse> GetStatsAsync();
}