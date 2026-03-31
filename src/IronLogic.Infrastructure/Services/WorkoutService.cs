using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Shared;
using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Provides services for creating and managing workout sessions from various sources.
/// </summary>
public class WorkoutService(
    IWorkoutParserService parserService,
    AppDbContext dbContext,
    ILogger<WorkoutService> logger)
    : IWorkoutService
{
    private static readonly Guid DefaultEquipmentId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultMuscleId = new("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// Creates a new workout session, including exercises and sets, from a raw text input.
    /// It also calculates personal record (PR) insights for each exercise.
    /// </summary>
    /// <param name="rawText">The raw text containing the workout log.</param>
    /// <param name="userId">The ID of the user performing the workout.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a <see cref="WorkoutImportResult"/> on success,
    /// which includes the new session ID and the parsed workout data with PR insights.
    /// Returns a failure result if parsing or saving fails.
    /// </returns>
    public async Task<Result<WorkoutImportResult>> CreateFromRawTextAsync(string rawText, string userId)
    {
        var parseResult = parserService.Parse(rawText);
        if (parseResult.IsFailure) return Result.Failure<WorkoutImportResult>(parseResult.Error);

        var workoutDto = parseResult.Value;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = workoutDto.Date,
                Title = workoutDto.Title
            };
            dbContext.Sessions.Add(session);

            var exercises = await GetOrCreateExercisesAsync(workoutDto.Exercises);
            await CalculatePrInsights(userId, workoutDto.Exercises, exercises);
            AddExerciseSessions(session.Id, workoutDto.Exercises, exercises);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var resultData = new WorkoutImportResult(session.Id, workoutDto);
            return Result.Success(resultData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while saving the workout to the database.");
            await transaction.RollbackAsync();
            return Result.Failure<WorkoutImportResult>("An error occurred while saving the workout to the database.");
        }
    }

    /// <summary>
    /// Retrieves existing exercises from the database or creates new ones if they don't exist.
    /// </summary>
    /// <param name="exerciseDtos">A collection of parsed exercise data transfer objects.</param>
    /// <returns>A dictionary mapping lowercase exercise names to their corresponding <see cref="Exercise"/> entities.</returns>
    private async Task<Dictionary<string, Exercise>> GetOrCreateExercisesAsync(
        ICollection<ParsedExerciseDto> exerciseDtos)
    {
        var exerciseNames = exerciseDtos
            .Select(e => e.Name.ToLower())
            .Distinct()
            .ToList();

        var existingExercises = await dbContext.Exercises
            .Where(e => exerciseNames.Contains(e.Name.ToLower()))
            .ToDictionaryAsync(e => e.Name.ToLower(), e => e);

        foreach (var exerciseDto in exerciseDtos)
        {
            var searchName = exerciseDto.Name.ToLower();
            if (!existingExercises.ContainsKey(searchName))
            {
                var newExercise = new Exercise
                {
                    Id = Guid.NewGuid(),
                    Name = exerciseDto.Name,
                    EquipmentId = DefaultEquipmentId,
                    PrimaryMuscleId = DefaultMuscleId
                };
                dbContext.Exercises.Add(newExercise);
                existingExercises[searchName] = newExercise;
            }
        }

        return existingExercises;
    }

    /// <summary>
    /// Calculates Personal Record (PR) insights for each exercise based on the user's historical performance.
    /// This method modifies the <see cref="ParsedExerciseDto.PrInsight"/> property of the DTOs.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="exerciseDtos">The list of parsed exercises for the current session.</param>
    /// <param name="exercises">A dictionary of the exercises involved in the session, mapping name to entity.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task CalculatePrInsights(string userId, IEnumerable<ParsedExerciseDto> exerciseDtos,
        IReadOnlyDictionary<string, Exercise> exercises)
    {
        var exerciseIds = exercises.Values.Select(e => e.Id).ToList();

        var userHistory = await dbContext.ExerciseSessions
            .Where(es => es.Session.UserId == userId && exerciseIds.Contains(es.ExerciseId))
            .Select(es => new { es.ExerciseId, es.Weight, es.Session.Date })
            .ToListAsync();

        var previousRecords = userHistory
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var maxRecord = g.OrderByDescending(x => x.Weight).ThenByDescending(x => x.Date).First();
                    return new { maxRecord.Weight, maxRecord.Date };
                }
            );

        foreach (var exerciseDto in exerciseDtos)
        {
            var exercise = exercises[exerciseDto.Name.ToLower()];
            var currentMaxWeight = exerciseDto.Sets.Count > 0 ? exerciseDto.Sets.Max(s => s.Weight) : 0;

            if (previousRecords.TryGetValue(exercise.Id, out var prevRecord))
            {
                if (currentMaxWeight > prevRecord.Weight)
                {
                    exerciseDto.PrInsight = new PrInsightDto(
                        IsNewRecord: true,
                        CurrentMaxWeight: currentMaxWeight,
                        PreviousMaxWeight: prevRecord.Weight,
                        PreviousDate: prevRecord.Date
                    );
                }
            }
            else if (currentMaxWeight > 0)
            {
                exerciseDto.PrInsight = new PrInsightDto(
                    IsNewRecord: true,
                    CurrentMaxWeight: currentMaxWeight,
                    PreviousMaxWeight: null,
                    PreviousDate: null
                );
            }
        }
    }

    /// <summary>
    /// Creates and adds <see cref="ExerciseSession"/> entities to the database context for each set in the workout.
    /// </summary>
    /// <param name="sessionId">The ID of the parent workout session.</param>
    /// <param name="exerciseDtos">The list of parsed exercises containing the sets to be added.</param>
    /// <param name="exercises">A dictionary of the exercises involved in the session, mapping name to entity.</param>
    private void AddExerciseSessions(Guid sessionId, IEnumerable<ParsedExerciseDto> exerciseDtos,
        IReadOnlyDictionary<string, Exercise> exercises)
    {
        foreach (var exerciseDto in exerciseDtos)
        {
            var exercise = exercises[exerciseDto.Name.ToLower()];
            foreach (var exerciseSession in exerciseDto.Sets.Select(setDto => new ExerciseSession
                     {
                         Id = Guid.NewGuid(),
                         SessionId = sessionId,
                         ExerciseId = exercise.Id,
                         SetIndex = setDto.SetIndex,
                         Weight = setDto.Weight,
                         Reps = setDto.Reps,
                         Rpe = setDto.Rpe
                     }))
            {
                dbContext.ExerciseSessions.Add(exerciseSession);
            }
        }
    }
}