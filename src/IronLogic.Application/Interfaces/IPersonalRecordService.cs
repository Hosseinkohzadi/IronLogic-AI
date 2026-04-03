using IronLogic.Application.DTOs;
using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

/// <summary>
///     Service for managing personal records and PR-related calculations.
/// </summary>
public interface IPersonalRecordService
{
    /// <summary>
    ///     Retrieves all-time personal records for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A dictionary mapping exercise names to their PR information.</returns>
    Task<Dictionary<string, PrInfo>> GetAllTimePrsAsync(string userId);

    /// <summary>
    ///     Calculates and populates PR insights for the given exercises.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="exerciseDtos">The collection of parsed exercise DTOs to analyze.</param>
    /// <param name="exercises">Dictionary of exercise entities mapped by name.</param>
    Task CalculatePrInsights(string userId, IEnumerable<ParsedExerciseDto> exerciseDtos,
        IReadOnlyDictionary<string, Exercise> exercises);

    /// <summary>
    ///     Invalidates the cached PR data for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    void InvalidateUserPrsCache(string userId);
}