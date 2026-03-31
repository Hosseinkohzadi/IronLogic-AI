using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Interfaces;
using IronLogic.Application.Shared;
using IronLogic.Domain.Entities;
using IronLogic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Services;

public class WorkoutService(IWorkoutParserService parserService, AppDbContext dbContext)
    : IWorkoutService
{
    private static readonly Guid DefaultEquipmentId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultMuscleId = new("00000000-0000-0000-0000-000000000002");

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

            var exerciseNames = workoutDto.Exercises
                .Select(e => e.Name.ToLower())
                .Distinct()
                .ToList();

            var existingExercises = await dbContext.Exercises
                .Where(e => exerciseNames.Contains(e.Name.ToLower()))
                .ToDictionaryAsync(e => e.Name.ToLower(), e => e);

            foreach (var exerciseDto in workoutDto.Exercises)
            {
                var searchName = exerciseDto.Name.ToLower();

                if (!existingExercises.TryGetValue(searchName, out var exercise))
                {
                    exercise = new Exercise
                    {
                        Id = Guid.NewGuid(),
                        Name = exerciseDto.Name,
                        EquipmentId = DefaultEquipmentId,
                        PrimaryMuscleId = DefaultMuscleId
                    };
                    dbContext.Exercises.Add(exercise);

                    existingExercises[searchName] = exercise;
                }

                foreach (var exerciseSession in exerciseDto.Sets.Select(setDto => new ExerciseSession
                         {
                             Id = Guid.NewGuid(),
                             SessionId = session.Id,
                             ExerciseId = exercise.Id,
                             SetIndex = setDto.SetIndex,
                             Weight = setDto.Weight,
                             Reps = setDto.Reps,
                             Rpe = setDto.Rpe
                         }))
                    dbContext.ExerciseSessions.Add(exerciseSession);
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