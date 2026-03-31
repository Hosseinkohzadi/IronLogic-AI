using System.Globalization;
using CsvHelper;
using IronLogic.Domain.Enums;
using IronLogic.Infrastructure.Mapper;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Provides services for importing workout data from external sources into the application.
/// </summary>
/// <param name="context">The database context for data access.</param>
public class WorkoutImportService(AppDbContext context) : IWorkoutImportService
{
    /// <summary>
    /// Imports workout sessions from a CSV file stream.
    /// This method parses the CSV data, creates new exercises if they don't exist,
    /// groups records into sessions, and saves them to the database.
    /// </summary>
    /// <param name="fileStream">The stream containing the workout data in CSV format.</param>
    /// <remarks>
    /// The user ID is currently hardcoded. This should be updated to use the
    /// authenticated user's ID in a production environment.
    /// </remarks>
    /// <returns>A task that represents the asynchronous import operation.</returns>
    public async Task ImportWorkoutsAsync(Stream fileStream)
    {
        var defaultUserId = "00000000-0000-0000-0000-000000000001";

        // 1. Check for user existence (if this is null, all sessions will face an FK error)
        var user = await context.Users.FindAsync(defaultUserId);
        if (user == null) throw new Exception("Default user not found in the database! Seed the database first.");

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
                Title = group.Key.Title ?? "Workout Session",
                User = user, // Direct link to the user object
                ExerciseSessions = new List<ExerciseSession>()
            };

            foreach (var record in group)
            {
                var exerciseName = record.ExerciseTitle.Trim();

                if (!exerciseCache.TryGetValue(exerciseName.ToLower(), out var exercise))
                {
                    exercise = new Exercise
                    {
                        Id = Guid.NewGuid(),
                        Name = exerciseName,
                        Type = ExerciseType.WeightAndReps,
                        PrimaryMuscle = new Muscle { Name = "General" },
                        Equipment = new Equipment { Name = "General" }
                    };
                    context.Exercises.Add(exercise);
                    exerciseCache.Add(exerciseName.ToLower(), exercise);
                }

                session.ExerciseSessions.Add(new ExerciseSession
                {
                    Exercise = exercise,
                    SetIndex = record.SetIndex,
                    SetType = record.SetType,
                    Reps = record.Reps,
                    Weight = record.WeightLbs,
                    DurationSeconds = record.DurationSeconds,
                    DistanceKm = record.DistanceKm,
                    Rpe = record.Rpe
                });
            }

            context.Sessions.Add(session);
        }

        // Save all records at once
        await context.SaveChangesAsync();
    }
}