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
    private const int ExercisesCacheDurationHours = 24;
    private const int PrsCacheDurationMinutes = 30;
    private const int Estimated1RmDivisor = 30;
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
        if (string.IsNullOrWhiteSpace(rawText))
            return Result.Failure<WorkoutImportResult>("Raw text cannot be empty.");

        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<WorkoutImportResult>("User ID is required.");

        var parseResult = parserService.Parse(rawText);
        if (parseResult.IsFailure) return Result.Failure<WorkoutImportResult>(parseResult.Error);

        var workoutDto = parseResult.Value;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
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

            var exercises = await GetOrCreateExercisesAsync(workoutDto.Exercises);
            await CalculatePrInsights(userId, sessionId, workoutDto.Exercises, exercises);
            AddExerciseSessions(sessionId, workoutDto.Exercises, exercises);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Invalidate PR cache after successful save
            InvalidateUserPrsCache(userId);

            var resultData = new WorkoutImportResult(sessionId, workoutDto);
            return Result.Success(resultData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while saving the workout for user {UserId}.", userId);
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
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<List<ExerciseHistoryPointDto>>("User ID is required.");

        if (string.IsNullOrWhiteSpace(exerciseName))
            return Result.Failure<List<ExerciseHistoryPointDto>>("Exercise name is required.");

        var exerciseNameLower = exerciseName.ToLower();

        var rawHistory = await dbContext.ExerciseSessions
            .AsNoTracking()
            .Where(es => es.Session.UserId == userId && es.Exercise.Name.ToLower() == exerciseNameLower)
            .Select(es => new { es.Session.Date, es.Weight, es.Reps, es.Rpe })
            .ToListAsync();

        if (!rawHistory.Any())
            return Result.Success(new List<ExerciseHistoryPointDto>());

        var history = rawHistory
            .GroupBy(es => es.Date.Date)
            .Select(g =>
            {
                var topSet = g.OrderByDescending(s => s.Weight).ThenByDescending(s => s.Reps).First();
                var rpeText = topSet.Rpe.HasValue ? $" @ {topSet.Rpe}" : string.Empty;
                var estimated1Rm = CalculateEstimated1Rm(topSet.Weight, topSet.Reps);

                return new ExerciseHistoryPointDto(
                    g.Key,
                    g.Max(s => s.Weight),
                    g.Sum(s => s.Weight * s.Reps),
                    $"{topSet.Weight} lbs x {topSet.Reps}{rpeText}",
                    estimated1Rm
                );
            })
            .OrderBy(h => h.Date)
            .ToList();

        return Result.Success(history);
    }

    /// <summary>
    ///     Retrieves all workout sessions for a specific user on a given date.
    /// </summary>
    public async Task<Result<List<DayDetailsDto>>> GetSessionsByDateAsync(string userId, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<List<DayDetailsDto>>("User ID is required.");

        var sessions = await dbContext.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Date.Date == date.Date)
            .Include(s => s.ExerciseSessions)
                .ThenInclude(es => es.Exercise)
            .ToListAsync();

        if (!sessions.Any())
            return Result.Success(new List<DayDetailsDto>());

        var allTimePrs = await GetAllTimePrsAsync(userId);

        var result = sessions.Select(s => new DayDetailsDto(
            s.Id,
            s.Title,
            s.Date,
            s.ExerciseSessions.Sum(es => es.Weight * es.Reps),
            s.ExerciseSessions
                .GroupBy(es => es.Exercise.Name)
                .Select(g => new ExerciseDetailDto(
                    g.Key,
                    g.OrderBy(x => x.SetIndex)
                        .Select(x => new SetDetailDto(
                            x.SetIndex,
                            x.Weight,
                            x.Reps,
                            x.Rpe,
                            allTimePrs.TryGetValue(g.Key, out var prInfo) && x.Weight == prInfo.MaxWeight
                        )).ToList()
                )).ToList()
        )).ToList();

        return Result.Success(result);
    }

    /// <summary>
    ///     Retrieves existing exercises from the Cache/Database or creates new ones if they don't exist.
    /// </summary>
    private async Task<Dictionary<string, Exercise>> GetOrCreateExercisesAsync(
        IEnumerable<ParsedExerciseDto> exerciseDtos)
    {
        if (!cache.TryGetValue(ExercisesCacheKey, out Dictionary<string, Exercise>? allExercises) || 
            allExercises == null)
        {
            allExercises = await dbContext.Exercises.ToDictionaryAsync(e => e.Name.ToLower(), e => e);
            cache.Set(ExercisesCacheKey, allExercises, CreateExercisesCacheOptions());
        }

        var result = new Dictionary<string, Exercise>();
        var isCacheDirty = false;

        foreach (var exerciseDto in exerciseDtos)
        {
            var searchName = exerciseDto.Name.ToLower();

            if (allExercises.TryGetValue(searchName, out var existingExercise))
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

                allExercises[searchName] = newExercise;
                isCacheDirty = true;
            }
        }

        if (isCacheDirty)
            cache.Set(ExercisesCacheKey, allExercises, CreateExercisesCacheOptions());

        return result;
    }

    private async Task CalculatePrInsights(string userId, Guid currentSessionId,
        IEnumerable<ParsedExerciseDto> exerciseDtos,
        IReadOnlyDictionary<string, Exercise> exercises)
    {
        // 🚀 Use cached PR data instead of direct database query
        var allTimePrs = await GetAllTimePrsAsync(userId);

        foreach (var exerciseDto in exerciseDtos)
        {
            var currentMaxWeight = exerciseDto.Sets.Count > 0 ? exerciseDto.Sets.Max(s => s.Weight) : 0;

            if (currentMaxWeight <= 0) continue;

            var exercise = exercises[exerciseDto.Name.ToLower()];

            // Check if we have historical PR data for this exercise
            if (allTimePrs.TryGetValue(exercise.Name, out var prInfo))
            {
                // Compare current max weight with historical PR
                if (currentMaxWeight > prInfo.MaxWeight)
                {
                    exerciseDto.PrInsight = new PrInsightDto(
                        true,
                        currentMaxWeight,
                        prInfo.MaxWeight,
                        prInfo.Date
                    );
                }
            }
            else
            {
                // First time doing this exercise - it's a PR!
                exerciseDto.PrInsight = new PrInsightDto(true, currentMaxWeight, null, null);
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

    private async Task<Dictionary<string, PrInfo>> GetAllTimePrsAsync(string userId)
    {
        var cacheKey = GetUserPrsCacheKey(userId);

        if (cache.TryGetValue(cacheKey, out Dictionary<string, PrInfo>? allTimePrs) && allTimePrs != null)
            return allTimePrs;

        // Fetch all exercise sessions with their weights and dates
        var exerciseHistory = await dbContext.ExerciseSessions
            .Where(es => es.Session.UserId == userId)
            .Select(es => new
            {
                ExerciseName = es.Exercise.Name,
                es.Weight,
                es.Session.Date
            })
            .ToListAsync();

        // Process in-memory to find max weight and corresponding date for each exercise
        allTimePrs = exerciseHistory
            .GroupBy(x => x.ExerciseName)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var maxWeight = g.Max(x => x.Weight) ?? 0;
                    var dateOfMax = g
                        .Where(x => x.Weight == maxWeight)
                        .OrderByDescending(x => x.Date)
                        .Select(x => x.Date)
                        .FirstOrDefault();
                    return new PrInfo(maxWeight, dateOfMax);
                }
            );

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(PrsCacheDurationMinutes));
        cache.Set(cacheKey, allTimePrs, cacheOptions);

        return allTimePrs;
    }

    private static decimal CalculateEstimated1Rm(decimal? weight, int? reps)
    {
        // Epley formula: 1RM = weight × (1 + reps/30)
        return Math.Round((weight ?? 0) * (1 + (reps ?? 0) / (decimal)Estimated1RmDivisor), 2);
    }

    private static string GetUserPrsCacheKey(string userId) => $"PRs_{userId}";

    private void InvalidateUserPrsCache(string userId)
    {
        cache.Remove(GetUserPrsCacheKey(userId));
    }

    private static MemoryCacheEntryOptions CreateExercisesCacheOptions() =>
        new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(ExercisesCacheDurationHours));

    /// <summary>
    ///     Record containing PR information for an exercise.
    /// </summary>
    /// <param name="MaxWeight">The maximum weight achieved.</param>
    /// <param name="Date">The date when the PR was achieved.</param>
    private record PrInfo(decimal MaxWeight, DateTime Date);
}