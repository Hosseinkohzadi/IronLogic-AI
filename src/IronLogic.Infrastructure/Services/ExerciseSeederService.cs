using System.Text.Json;
using Exercise = IronLogic.Domain.Entities.Exercise;

namespace IronLogic.Infrastructure.Services;

/// <summary>
///     Service responsible for seeding exercise data from JSON files into the database.
/// </summary>
public static class ExerciseSeederService
{
    /// <summary>
    ///     Seeds exercise data from a JSON file into the database.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SeedAsync(AppDbContext context)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "exercises_final.json");
        if (!File.Exists(filePath)) return;

        var jsonData = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawData = JsonSerializer.Deserialize<List<ExerciseJsonModel>>(jsonData, options);

        if (rawData == null || !rawData.Any()) return;

        // Step 1: Ensure base data exists (muscles and equipment)
        await EnsureBaseDataExistsAsync(context, rawData);

        // If exercises already exist, stop
        if (await context.Exercises.AnyAsync()) return;

        // Step 2: Reload maps after saving the first stage
        var muscleMap = await context.Muscles.ToDictionaryAsync(m => m.Name.ToLower(), m => m);
        var equipmentMap = await context.Equipments.ToDictionaryAsync(e => e.Name.ToLower(), e => e.Id);

        // Get default IDs for fallback
        var defaultMuscleId = muscleMap.Values.First(x => x.Name.Equals("General", StringComparison.OrdinalIgnoreCase)).Id;
        var defaultEquipId = equipmentMap.Keys.Contains("none") ? equipmentMap["none"] : equipmentMap.Values.First();

        // Step 3: Add exercises with handling for missing values
        foreach (var item in rawData)
        {
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Url = item.Url,
                ImagePath = item.Image_Path,
                Mechanics = item.Mechanics ?? "None",
                Instructions = item.Instructions != null ? string.Join("\n", item.Instructions) : "",
                SecondaryMuscles = new List<Muscle>()
            };

            // Safe assignment of primary muscle
            var pMuscleName = (item.Primary_Muscle ?? "General").ToLower();
            exercise.PrimaryMuscleId = muscleMap.TryGetValue(pMuscleName, out var pm) ? pm.Id : defaultMuscleId;

            // Safe assignment of equipment
            var equipName = (item.Equipment ?? "None").ToLower();
            exercise.EquipmentId = equipmentMap.TryGetValue(equipName, out var eid) ? eid : defaultEquipId;

            // Assign secondary muscles
            if (!string.IsNullOrWhiteSpace(item.Secondary_Muscle) && item.Secondary_Muscle != "None")
            {
                var secondaryNames = item.Secondary_Muscle.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var sName in secondaryNames)
                {
                    if (muscleMap.TryGetValue(sName.Trim().ToLower(), out var secMuscle))
                    {
                        exercise.SecondaryMuscles.Add(secMuscle);
                    }
                }
            }

            context.Exercises.Add(exercise);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Ensures that base data (muscles and equipment) exists in the database before seeding exercises.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="data">The raw exercise data from JSON.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task EnsureBaseDataExistsAsync(AppDbContext context, List<ExerciseJsonModel> data)
    {
        // Extract names and ensure "General" entity exists
        var muscleNames = data.Select(x => x.Primary_Muscle)
            .Concat(data.SelectMany(x => (x.Secondary_Muscle ?? "").Split(',').Select(s => s.Trim())))
            .Append("General") // Ensure default entity exists
            .Where(x => !string.IsNullOrEmpty(x) && x != "None")
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var equipNames = data.Select(x => x.Equipment)
            .Append("None") // Ensure default equipment exists
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var existingMuscles = await context.Muscles.Select(m => m.Name.ToLower()).ToListAsync();
        foreach (var name in muscleNames.Where(n => !existingMuscles.Contains(n.ToLower())))
        {
            context.Muscles.Add(new Muscle { Id = Guid.NewGuid(), Name = name });
        }

        var existingEquip = await context.Equipments.Select(e => e.Name.ToLower()).ToListAsync();
        foreach (var name in equipNames.Where(n => !existingEquip.Contains(n.ToLower())))
        {
            context.Equipments.Add(new Equipment { Id = Guid.NewGuid(), Name = name });
        }

        await context.SaveChangesAsync(); // CRITICAL: Save before next stage
    }

    /// <summary>
    ///     Model representing exercise data from JSON file.
    /// </summary>
    private class ExerciseJsonModel
    {
        /// <summary>
        ///     Gets or sets the name of the exercise.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the URL reference for the exercise.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     Gets or sets the image path for the exercise.
        /// </summary>
        public string Image_Path { get; set; }

        /// <summary>
        ///     Gets or sets the primary muscle targeted by the exercise.
        /// </summary>
        public string Primary_Muscle { get; set; }

        /// <summary>
        ///     Gets or sets the secondary muscles targeted by the exercise.
        /// </summary>
        public string Secondary_Muscle { get; set; }

        /// <summary>
        ///     Gets or sets the equipment required for the exercise.
        /// </summary>
        public string Equipment { get; set; }

        /// <summary>
        ///     Gets or sets the mechanics type of the exercise.
        /// </summary>
        public string Mechanics { get; set; }

        /// <summary>
        ///     Gets or sets the list of instructions for performing the exercise.
        /// </summary>
        public List<string> Instructions { get; set; }
    }
}