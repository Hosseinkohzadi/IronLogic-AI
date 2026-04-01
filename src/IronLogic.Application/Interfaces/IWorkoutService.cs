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

    /// <summary>
    /// Retrieves all workout sessions for a specific user on a given date, including exercise and set details.
    /// </summary>
    /// <param name="userId">The ID of the user whose sessions are being retrieved.</param>
    /// <param name="date">The date for which to retrieve workout sessions.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/>
    /// with a list of <see cref="DayDetailsDto"/> containing session details, or an error on failure.
    /// </returns>
    Task<Result<List<DayDetailsDto>>> GetSessionsByDateAsync(string userId, DateTime date);

    /// <summary>
    /// Retrieves the performance history for a specific exercise for a given user,
    /// including weight progression, volume, and estimated one-rep max calculations.
    /// </summary>
    /// <param name="userId">The ID of the user whose exercise history is being retrieved.</param>
    /// <param name="exerciseName">The name of the exercise to retrieve history for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/>
    /// with a list of <see cref="ExerciseHistoryPointDto"/> representing historical performance data,
    /// or an error on failure.
    /// </returns>
    Task<Result<List<ExerciseHistoryPointDto>>> GetExerciseHistoryAsync(string userId, string exerciseName);
}