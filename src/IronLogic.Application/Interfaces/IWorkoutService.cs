using IronLogic.Application.DTOs;
using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Shared;

namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines the contract for a service that handles workout-related operations,
/// such as creating workout sessions from raw text.
/// </summary>
public interface IWorkoutService
{
    /// <summary>
    /// Parses a raw text string representing a workout, creates the corresponding domain entities,
    /// and persists them to the database.
    /// </summary>
    /// <param name="rawText">The raw string input containing the workout log.</param>
    /// <param name="userId">The ID of the user who owns the workout session.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/>
    /// indicating the success or failure of the operation. On success, it returns a <see cref="WorkoutImportResult"/>
    /// with the ID of the newly created session.
    /// </returns>
    Task<Result<WorkoutImportResult>> CreateFromRawTextAsync(string rawText, string userId);

    Task<Result<List<DayDetailsDto>>> GetSessionsByDateAsync(string userId, DateTime date);
}