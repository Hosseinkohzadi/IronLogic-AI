using IronLogic.Application.DTOs;
using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Shared;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure.Services;

/// <summary>
///     Provides services for creating and managing workout sessions from various sources.
/// </summary>
public class WorkoutService(
    IWorkoutParserService parserService,
    AppDbContext dbContext,
    ILogger<WorkoutService> logger,
    IMemoryCache cache)
    : IWorkoutService
{
    private const string ExercisesCacheKey = "AllExercises_Cache";
    private static readonly Guid DefaultEquipmentId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultMuscleId = new("00000000-0000-0000-0000-000000000001");

    /// <summary>
    ///     Parses a raw text input to create a new workout session or update an existing one for a specific user.
    /// </summary>
    /// <param name="rawText">The raw text representing the workout data.</param>
    /// <param name="userId">The ID of the user for whom the workout is being created.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing a <see cref="WorkoutImportResult" /> on success,
    ///     or an error message on failure.
    /// </returns>
    public async Task<Result<WorkoutImportResult>> CreateFromRawTextAsync(string rawText, string userId)
    {
        var parseResult = parserService.Parse(rawText);
        if (parseResult.IsFailure) return Result.Failure<WorkoutImportResult>(parseResult.Error);

        var workoutDto = parseResult.Value;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // 🚀 1. Upsert Logic: Check for an existing session with the same date and title.
            var existingSession = await dbContext.Sessions
                .Include(s => s.ExerciseSessions)
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId && s.Date == workoutDto.Date && s.Title == workoutDto.Title);

            Guid sessionId;

            if (existingSession != null)
            {
                sessionId = existingSession.Id;
                // If it exists, remove the old sets to be replaced with new data.
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

            var exercises = await GetOrCreateExercisesAsync(workoutDto.Exercises);

            // Also pass the session ID to avoid comparing records of the same session against itself during an update.
            await CalculatePrInsights(userId, sessionId, workoutDto.Exercises, exercises);

            AddExerciseSessions(sessionId, workoutDto.Exercises, exercises);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var resultData = new WorkoutImportResult(sessionId, workoutDto);
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
    ///     Retrieves the performance history for a specific exercise for a given user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="exerciseName">The name of the exercise.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing a list of <see cref="ExerciseHistoryPointDto" /> on success,
    ///     or an error on failure.
    /// </returns>
    public async Task<Result<List<ExerciseHistoryPointDto>>> GetExerciseHistoryAsync(string userId, string exerciseName)
    {
        var exerciseNameLower = exerciseName.ToLower();

        // 1. Fetch only the necessary fields from the database (very fast).
        var rawHistory = await dbContext.ExerciseSessions
            .AsNoTracking()
            .Where(es => es.Session.UserId == userId && es.Exercise.Name.ToLower() == exerciseNameLower)
            .Select(es => new { es.Session.Date, es.Weight, es.Reps, es.Rpe })
            .ToListAsync();

        // 2. Grouping and calculations are done in client-side memory.
        var history = rawHistory
            .GroupBy(es => es.Date.Date)
            .Select(g =>
            {
                // Find the best set of that day.
                var topSet = g.OrderByDescending(s => s.Weight).ThenByDescending(s => s.Reps).First();
                var rpeText = topSet.Rpe.HasValue ? $" @ {topSet.Rpe}" : "";

                return new ExerciseHistoryPointDto(
                    g.Key,
                    g.Max(s => s.Weight),
                    g.Sum(s => s.Weight * s.Reps),
                    $"{topSet.Weight} lbs x {topSet.Reps}{rpeText}",
                    Math.Round((decimal)g.Max(s => s.Weight * (1 + s.Reps / 30m)), 2)
                );
            })
            .OrderBy(h => h.Date)
            .ToList();

        return Result.Success(history);
    }

    /// <summary>
    ///     Retrieves existing exercises from the Cache/Database or creates new ones if they don't exist.
    /// </summary>
    private async Task<Dictionary<string, Exercise>> GetOrCreateExercisesAsync(
        IEnumerable<ParsedExerciseDto> exerciseDtos)
    {
        // 🚀 2. Caching Logic: Get all exercises from the server cache instead of the database.
        if (!cache.TryGetValue(ExercisesCacheKey, out Dictionary<string, Exercise>? allExercises))
        {
            allExercises = await dbContext.Exercises.ToDictionaryAsync(e => e.Name.ToLower(), e => e);

            var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(24));
            cache.Set(ExercisesCacheKey, allExercises, cacheOptions);
        }

        var result = new Dictionary<string, Exercise>();
        var isCacheDirty = false;

        foreach (var exerciseDto in exerciseDtos)
        {
            var searchName = exerciseDto.Name.ToLower();

            // Search in cache (O(1) runtime)
            if (allExercises!.TryGetValue(searchName, out var existingExercise))
            {
                result[searchName] = existingExercise;
            }
            else
            {
                var newExercise = new Exercise
                {
                    Id = Guid.NewGuid(),
                    Name = exerciseDto.Name,
                    EquipmentId = DefaultEquipmentId,
                    PrimaryMuscleId = DefaultMuscleId
                };
                dbContext.Exercises.Add(newExercise);
                result[searchName] = newExercise;

                // Add the new exercise to the cached list
                allExercises[searchName] = newExercise;
                isCacheDirty = true;
            }
        }

        // If a new exercise was added, update the cache.
        if (isCacheDirty)
            cache.Set(ExercisesCacheKey, allExercises,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(24)));

        return result;
    }

    private async Task CalculatePrInsights(string userId, Guid currentSessionId,
        IEnumerable<ParsedExerciseDto> exerciseDtos,
        IReadOnlyDictionary<string, Exercise> exercises)
    {
        var exerciseIds = exercises.Values.Select(e => e.Id).ToList();

        // 🚀 Note: Added condition es.SessionId != currentSessionId
        // This prevents the previous records of the same session from being included in the history calculation when "updating" a workout.
        var userHistory = await dbContext.ExerciseSessions
            .Where(es => es.Session.UserId == userId
                         && es.SessionId != currentSessionId
                         && exerciseIds.Contains(es.ExerciseId))
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
                    exerciseDto.PrInsight = new PrInsightDto(
                        true,
                        currentMaxWeight,
                        prevRecord.Weight,
                        prevRecord.Date
                    );
            }
            else if (currentMaxWeight > 0)
            {
                exerciseDto.PrInsight = new PrInsightDto(
                    true,
                    currentMaxWeight,
                    null,
                    null
                );
            }
        }
    }

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
                dbContext.ExerciseSessions.Add(exerciseSession);
        }
    }
}