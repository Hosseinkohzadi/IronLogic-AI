using IronLogic.Application.DTOs;

namespace IronLogic.Application.Interfaces;

/// <summary>
///     Abstraction for external workout providers (e.g., Hevy) returning normalized DTOs.
/// </summary>
public interface IWorkoutProvider
{
    Task<IEnumerable<HevyWorkoutSessionDto>> GetRecentSessionsAsync(int limit = 10);
}