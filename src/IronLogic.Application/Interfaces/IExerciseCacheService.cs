using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

/// <summary>
///     Service for managing exercise caching and retrieval.
/// </summary>
public interface IExerciseCacheService
{
    /// <summary>
    ///     Retrieves existing exercises from cache/database or creates new ones if they don't exist.
    /// </summary>
    /// <param name="exerciseDtos">The collection of parsed exercise DTOs.</param>
    /// <returns>A dictionary mapping exercise names (lowercase) to Exercise entities.</returns>
    Task<Dictionary<string, Exercise>> GetOrCreateExercisesAsync(IEnumerable<ParsedExerciseDto> exerciseDtos);
}