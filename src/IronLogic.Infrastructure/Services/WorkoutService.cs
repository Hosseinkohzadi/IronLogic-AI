using IronLogic.Application.DTOs;
using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Shared;
using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure.Services;

/// <summary>
///     Provides services for creating and managing workout sessions from various sources.
/// </summary>
public class WorkoutService(
    IWorkoutParserService parserService,
    IExerciseCacheService exerciseCacheService,
    IPersonalRecordService personalRecordService,
    IWorkoutPersistenceService persistenceService,
    AppDbContext dbContext,
    ILogger<WorkoutService> logger)
    : IWorkoutService
{
    private const int Estimated1RmDivisor = 30;

    /// <summary>
    ///     Parses a raw text input to create a new workout session or update an existing one for a specific user.
    /// </summary>
    public async Task<Result<WorkoutImportResult>> CreateFromRawTextAsync(string rawText, string userId)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return Result.Failure<WorkoutImportResult>("Raw text cannot be empty.");

        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<WorkoutImportResult>("User ID is required.");

        var parseResult = parserService.Parse(rawText);
        if (parseResult.IsFailure)
            return Result.Failure<WorkoutImportResult>(parseResult.Error);

        var workoutDto = parseResult.Value;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var sessionId = await persistenceService.CreateOrUpdateSessionAsync(userId, workoutDto);
            var exercises = await exerciseCacheService.GetOrCreateExercisesAsync(workoutDto.Exercises);
            await personalRecordService.CalculatePrInsights(userId, workoutDto.Exercises, exercises);
            persistenceService.AddExerciseSessions(sessionId, workoutDto.Exercises, exercises);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            personalRecordService.InvalidateUserPrsCache(userId);

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
            .ThenInclude(e => e.PrimaryMuscle) // 🚀 Load the PrimaryMuscle navigation property
            .ToListAsync();

        if (!sessions.Any())
            return Result.Success(new List<DayDetailsDto>());

        var allTimePrs = await personalRecordService.GetAllTimePrsAsync(userId);

        var result = sessions.Select(s => new DayDetailsDto(
            s.Id,
            s.Title,
            s.Date,
            s.ExerciseSessions.Sum(es => es.Weight * es.Reps),
            s.ExerciseSessions
                .GroupBy(es => es.Exercise.Name)
                .Select(g => new ExerciseDetailDto(
                    g.Key,
                    g.First().Exercise.PrimaryMuscle?.Name ?? "Unknown", // 🚀 Get the muscle name from the database
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

    private static decimal CalculateEstimated1Rm(decimal? weight, int? reps)
    {
        // Epley formula: 1RM = weight × (1 + reps/30)
        return Math.Round((weight ?? 0) * (1 + (reps ?? 0) / (decimal)Estimated1RmDivisor), 2);
    }
}