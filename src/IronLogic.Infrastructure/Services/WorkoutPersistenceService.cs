using IronLogic.Application.DTOs.ParsedWorkout;

namespace IronLogic.Infrastructure.Services;

/// <summary>
///     Handles persistence of workout sessions and exercise sessions.
/// </summary>
public class WorkoutPersistenceService(AppDbContext dbContext) : IWorkoutPersistenceService
{
    public async Task<Guid> CreateOrUpdateSessionAsync(string userId, ParsedWorkoutDto workoutDto)
    {
        var existingSession = await dbContext.Sessions
            .Include(s => s.ExerciseSessions)
            .FirstOrDefaultAsync(s =>
                s.UserId == userId && s.Date == workoutDto.Date && s.Title == workoutDto.Title);

        Guid sessionId;

        if (existingSession != null)
        {
            sessionId = existingSession.Id;
            dbContext.ExerciseSessions.RemoveRange(existingSession.ExerciseSessions);
        }
        else
        {
            sessionId = Guid.NewGuid();
            var session = new Session
            {
                Id = sessionId,
                UserId = userId,
                Date = workoutDto.Date,
                Title = workoutDto.Title
            };
            dbContext.Sessions.Add(session);
        }

        return sessionId;
    }

    public void AddExerciseSessions(Guid sessionId, IEnumerable<ParsedExerciseDto> exerciseDtos,
        IReadOnlyDictionary<string, Exercise> exercises)
    {
        foreach (var exerciseDto in exerciseDtos)
        {
            var exercise = exercises[exerciseDto.Name.ToLower()];
            foreach (var setDto in exerciseDto.Sets)
            {
                var exerciseSession = new ExerciseSession
                {
                    Id = Guid.NewGuid(),
                    SessionId = sessionId,
                    ExerciseId = exercise.Id,
                    SetIndex = setDto.SetIndex,
                    Weight = setDto.Weight,
                    Reps = setDto.Reps,
                    Rpe = setDto.Rpe
                };
                dbContext.ExerciseSessions.Add(exerciseSession);
            }
        }
    }
}