using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Interfaces;
using IronLogic.Application.Shared;
using IronLogic.Domain.Entities;
using IronLogic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Implements the application service for creating workout sessions from raw text.
/// </summary>
public class WorkoutService(IWorkoutParserService parserService, AppDbContext dbContext)
    : IWorkoutService
{
    private static readonly Guid DefaultEquipmentId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultMuscleId = new("00000000-0000-0000-0000-000000000002");

    /// <summary>
    /// Parses a raw text workout log, creates domain entities, and saves them to the database.
    /// </summary>
    /// <param name="rawText">The raw text log of the workout.</param>
    /// <param name="userId">The ID of the user who performed the workout.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a <see cref="WorkoutImportResult"/> on success,
    /// which includes the new session ID and the parsed data. On failure, it returns an error message.
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

            foreach (var exerciseDto in workoutDto.Exercises)
            {
                var exercise = await dbContext.Exercises
                    .FirstOrDefaultAsync(e => e.Name.ToLower() == exerciseDto.Name.ToLower());

                if (exercise == null)
                {
                    exercise = new Exercise
                    {
                        Id = Guid.NewGuid(),
                        Name = exerciseDto.Name,
                        EquipmentId = DefaultEquipmentId,
                        PrimaryMuscleId = DefaultMuscleId
                    };
                    dbContext.Exercises.Add(exercise);
                }

                foreach (var setDto in exerciseDto.Sets)
                {
                    var exerciseSession = new ExerciseSession
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        ExerciseId = exercise.Id,
                        SetIndex = setDto.SetIndex,
                        Weight = setDto.Weight,
                        Reps = setDto.Reps,
                        Rpe = setDto.Rpe
                    };
                    dbContext.ExerciseSessions.Add(exerciseSession);
                }
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var resultData = new WorkoutImportResult(session.Id, workoutDto);
            return Result.Success(resultData);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Result.Failure<WorkoutImportResult>("An error occurred while saving the workout to the database.");
        }
    }
}