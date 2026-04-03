using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

/// <summary>
///     Service for handling workout session persistence operations.
/// </summary>
public interface IWorkoutPersistenceService
{
    /// <summary>
    ///     Creates a new workout session or updates an existing one.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="workoutDto">The parsed workout data.</param>
    /// <returns>The ID of the created or updated session.</returns>
    Task<Guid> CreateOrUpdateSessionAsync(string userId, ParsedWorkoutDto workoutDto);

    /// <summary>
    ///     Adds exercise sessions to the database for a specific workout session.
    /// </summary>
    /// <param name="sessionId">The ID of the workout session.</param>
    /// <param name="exerciseDtos">The collection of parsed exercise DTOs.</param>
    /// <param name="exercises">Dictionary of exercise entities mapped by name.</param>
    void AddExerciseSessions(Guid sessionId, IEnumerable<ParsedExerciseDto> exerciseDtos,
        IReadOnlyDictionary<string, Exercise> exercises);
}