using IronLogic.Application.DTOs;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

/// <summary>
///     Defines the repository for managing workout session data.
/// </summary>
public interface IWorkoutSessionRepository
{
    /// <summary>
    ///     Retrieves a specific workout session by its unique identifier, including related exercise details.
    /// </summary>
    /// <param name="id">The unique identifier of the workout session.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the <see cref="Session" /> if
    ///     found; otherwise, null.
    /// </returns>
    Task<Session?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Retrieves all workout sessions for a specific user, ordered by date descending.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of
    ///     <see cref="WorkoutResponseDto" />.
    /// </returns>
    Task<List<WorkoutResponseDto>> GetAllByUserIdAsync(string userId);

    /// <summary>
    ///     Retrieves aggregated workout statistics for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the
    ///     <see cref="WorkoutStatsResponseDto" />.
    /// </returns>
    Task<WorkoutStatsResponseDto> GetWorkoutStatsAsync(string userId);

    /// <summary>
    ///     Retrieves a list of workout sessions with their details for a specific user, optionally filtered by a start date.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="startDate">An optional start date to filter the sessions.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of <see cref="Session" />
    ///     objects.
    /// </returns>
    Task<List<Session>> GetSessionsWithDetailsAsync(string userId, DateTime? startDate = null);

    /// <summary>
    ///     Adds a new workout session to the data store.
    /// </summary>
    /// <param name="session">The <see cref="Session" /> to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Add(Session session);

    /// <summary>
    ///     Marks a workout session as modified in the data store.
    /// </summary>
    /// <param name="session">The <see cref="Session" /> to update.</param>
    void Update(Session session);

    /// <summary>
    ///     Marks a workout session for deletion from the data store.
    /// </summary>
    /// <param name="session">The <see cref="Session" /> to delete.</param>
    void Delete(Session session);

    /// <summary>
    ///     Saves all changes made in this context to the underlying database.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is true if at least one record was changed,
    ///     otherwise false.
    /// </returns>
    Task<bool> SaveChangesAsync();

    /// <summary>
    ///     Retrieves the weekly workout volume trend for a user over a specified period.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="twelveWeeksAgo">The start date for the trend calculation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an object with the weekly trend
    ///     data.
    /// </returns>
    Task<object> GetWeeklyVolumeTrend(string userId, DateTime twelveWeeksAgo);
}