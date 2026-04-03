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
        var generalId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // 🚀 مرحله اول: چک کردن و ساخت موجودیت‌های پایه اگر وجود ندارند
        var hasGeneralMuscle = await context.Muscles.AnyAsync(m => m.Id == generalId);
        if (!hasGeneralMuscle)
        {
            context.Muscles.Add(new Muscle { Id = generalId, Name = "General" });
        }

        var hasGeneralEquip = await context.Equipments.AnyAsync(e => e.Id == generalId);
        if (!hasGeneralEquip)
        {
            context.Equipments.Add(new Equipment { Id = generalId, Name = "General" });
        }

        // اگر تغییری در مرحله قبل بود، ذخیره کن تا IDها معتبر شوند
        await context.SaveChangesAsync();

        var user = await context.Users.FindAsync(defaultUserId);
        if (user == null) throw new Exception("Default user not found!");

        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<ExerciseRecordMap>();

        var records = csv.GetRecords<ExerciseRecordDto>()
            .Where(r => !string.IsNullOrWhiteSpace(r.ExerciseTitle)).ToList();

        // لود کردن تمرینات برای جستجوی فازی
        var allExercises = await context.Exercises.ToListAsync();
        var groupedSessions = records.GroupBy(r => new { r.Title, r.StartTime });

        foreach (var group in groupedSessions)
        {
            var session = new Session
            {
                Date = group.Key.StartTime,
                Title = group.Key.Title ?? "Workout Session",
                UserId = defaultUserId,
                ExerciseSessions = new List<ExerciseSession>()
            };

            foreach (var record in group)
            {
                var exerciseName = record.ExerciseTitle.Trim();

                // 🚀 اصلاح منطق جستجو و جلوگیری از تکرار
                var exercise = FindOrCreateExercise(allExercises, exerciseName, generalId);

                session.ExerciseSessions.Add(new ExerciseSession
                {
                    Exercise = exercise,
                    SetIndex = record.SetIndex,
                    SetType = record.SetType,
                    Reps = record.Reps,
                    Weight = record.WeightLbs,
                    Rpe = record.Rpe
                    // سایر فیلدها...
                });
            }

            context.Sessions.Add(session);
        }

        await context.SaveChangesAsync();
    }

    private Exercise FindOrCreateExercise(List<Exercise> allExercises, string name, Guid generalId)
    {
        // ۱. ابتدا جستجوی دقیق (Case-insensitive) در لیست فعلی
        var existing = allExercises.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (existing != null) return existing;

        // ۲. جستجوی فازی
        var bestMatch = allExercises
            .Select(ex =>
            {
                var cleanedDbName = System.Text.RegularExpressions.Regex
                    .Replace(ex.Name.ToLower(), @"\s*\(.*?\)", "")
                    .Trim();

                var cleanedInputName = System.Text.RegularExpressions.Regex
                    .Replace(name.ToLower(), @"\s*\(.*?\)", "")
                    .Trim();

                return new
                {
                    Ex = ex,
                    Score = ExtensionMethods.CalculateDiceSimilarity(cleanedInputName, cleanedDbName)
                };
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();
        if (bestMatch != null && bestMatch.Score > 0.7)
            return bestMatch.Ex;

        // ۳. اگر پیدا نشد، تمرین جدید بساز و به لیست "allExercises" اضافه کن تا برای رکوردهای بعدی در دسترس باشد
        var newEx = new Exercise
        {
            Name = name,
            PrimaryMuscleId = generalId, 
            EquipmentId = generalId,
            Type = ExerciseType.WeightAndReps,
            HowTo = null,
            Image =
            [
            ],
            LinkOfVideo = "---",
            ImagePath = "assets/exercises/general.webp",
            Instructions = "None",
            Mechanics = "None",
            Url = "None",

        };

        context.Exercises.Add(newEx); // اضافه کردن به Context
        allExercises.Add(newEx); // 🚀 اضافه کردن به لیست محلی برای استفاده در ردیف‌های بعدی CSV
        return newEx;
    }
}