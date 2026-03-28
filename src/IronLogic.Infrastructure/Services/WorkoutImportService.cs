using System.Globalization;
using CsvHelper;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;
using IronLogic.Infrastructure.Data;
using IronLogic.Infrastructure.Mapper;
using Microsoft.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Services;

public class WorkoutImportService(AppDbContext context) : IWorkoutImportService
{
    public async Task ImportWorkoutsAsync(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<ExerciseRecordMap>();

        var records = csv.GetRecords<ExerciseRecordDto>()
            .Where(r => !string.IsNullOrWhiteSpace(r.ExerciseTitle))
            .ToList();

        var exerciseCache = await context.Exercises.ToDictionaryAsync(e => e.Name.ToLower(), e => e);

        var groupedSessions = records.GroupBy(r => new { r.Title, r.StartTime });

        foreach (var group in groupedSessions)
        {
            var session = new Session
            {
                Date = group.Key.StartTime,
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                ExerciseSessions = new List<ExerciseSession>()
            };

            foreach (var record in group)
            {
                var exerciseName = record.ExerciseTitle.Trim();

                if (!exerciseCache.TryGetValue(exerciseName.ToLower(), out var exercise))
                {
                    exercise = new Exercise { Name = exerciseName, Type = ExerciseType.WeightAndReps };
                    context.Exercises.Add(exercise);
                    exerciseCache.Add(exerciseName.ToLower(), exercise);
                }

                session.ExerciseSessions.Add(new ExerciseSession
                {
                    ExerciseId = exercise.Id,
                    SetIndex = record.SetIndex,
                    SetType = record.SetType,
                    Reps = record.Reps,
                    Weight = record.WeightLbs,
                    DistanceKm = record.DistanceKm,
                    DurationSeconds = record.DurationSeconds,
                    Rpe = record.Rpe
                });
            }

            context.Sessions.Add(session);
        }

        await context.SaveChangesAsync();
    }
}