using IronLogic.Application.DTOs.ParsedWorkout;
using Microsoft.Extensions.Caching.Memory;

namespace IronLogic.Infrastructure.Services;

/// <summary>
///     Manages caching and retrieval of exercises.
/// </summary>
public class ExerciseCacheService(
    IMemoryCache cache,
    AppDbContext dbContext,
    IMuscleMapperService muscleMapper) : IExerciseCacheService
{
    private const string ExercisesCacheKey = "AllExercises_Cache";
    private const int ExercisesCacheDurationHours = 24;
    private static readonly Guid DefaultEquipmentId = new("00000000-0000-0000-0000-000000000001");

    public async Task<Dictionary<string, Exercise>> GetOrCreateExercisesAsync(
        IEnumerable<ParsedExerciseDto> exerciseDtos)
    {
        if (!cache.TryGetValue(ExercisesCacheKey, out Dictionary<string, Exercise>? allExercises) ||
            allExercises == null)
        {
            allExercises = await dbContext.Exercises.ToDictionaryAsync(e => e.Name.ToLower(), e => e);
            cache.Set(ExercisesCacheKey, allExercises, CreateCacheOptions());
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
                // 🚀 Intelligent muscle detection when creating new exercise
                var (primaryMuscleId, secondaryMuscleId) = muscleMapper.MapMuscles(exerciseDto.Name);

                var newExercise = new Exercise
                {
                    Id = Guid.NewGuid(),
                    Name = exerciseDto.Name,
                    EquipmentId = DefaultEquipmentId,
                    PrimaryMuscleId = primaryMuscleId,
                    SecondaryMuscles = new List<Muscle>()
                };

                // If there's a secondary muscle, load it from the database and add to the collection
                if (secondaryMuscleId.HasValue)
                {
                    var secondaryMuscle = await dbContext.Muscles.FindAsync(secondaryMuscleId.Value);
                    if (secondaryMuscle != null) newExercise.SecondaryMuscles.Add(secondaryMuscle);
                }

                dbContext.Exercises.Add(newExercise);
                result[searchName] = newExercise;

                allExercises[searchName] = newExercise;
                isCacheDirty = true;
            }
        }

        if (isCacheDirty)
            cache.Set(ExercisesCacheKey, allExercises, CreateCacheOptions());

        return result;
    }

    private static MemoryCacheEntryOptions CreateCacheOptions()
    {
        return new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(ExercisesCacheDurationHours));
    }
}

/// <summary>
///     Service for mapping exercise names to primary and secondary muscle groups.
/// </summary>
public class MuscleMapperService : IMuscleMapperService
{
    /// <summary>
    ///     Maps an exercise name to its primary and optional secondary muscle groups.
    /// </summary>
    /// <param name="exerciseName">The name of the exercise.</param>
    /// <returns>A tuple containing the primary muscle ID and optional secondary muscle ID.</returns>
    public (Guid PrimaryMuscleId, Guid? SecondaryMuscleId) MapMuscles(string exerciseName)
    {
        var name = exerciseName.ToLower();

        // Chest exercises
        if (ContainsAny(name, "bench press", "chest press", "fly", "flye", "push up", "pushup", "pec deck", "dip"))
            return (MuscleIds.Chest, MuscleIds.Triceps);

        // Leg exercises - Quads dominant
        if (ContainsAny(name, "squat", "leg press", "lunge", "leg extension", "front squat", "hack squat"))
            return (MuscleIds.Quads, MuscleIds.Glutes);

        // Leg exercises - Hamstrings/Glutes dominant
        if (ContainsAny(name, "deadlift", "leg curl", "hamstring", "romanian deadlift", "rdl", "good morning"))
            return (MuscleIds.Hamstrings, MuscleIds.Glutes);

        // Glute-focused exercises
        if (ContainsAny(name, "hip thrust", "glute bridge", "kickback"))
            return (MuscleIds.Glutes, MuscleIds.Hamstrings);

        // Back exercises
        if (ContainsAny(name, "pull up", "pullup", "chin up", "row", "lat pulldown", "pull down", "back extension"))
            return (MuscleIds.Back, MuscleIds.Biceps);

        // Shoulder exercises
        if (ContainsAny(name, "shoulder press", "overhead press", "military press", "lateral raise", "front raise",
                "arnold press", "upright row"))
            return (MuscleIds.Shoulders, MuscleIds.Triceps);

        // Bicep exercises
        if (ContainsAny(name, "bicep curl", "hammer curl", "preacher curl", "concentration curl", "cable curl"))
            return (MuscleIds.Biceps, MuscleIds.Forearms);

        // Tricep exercises
        if (ContainsAny(name, "tricep extension", "skullcrusher", "tricep pushdown", "overhead extension",
                "close grip bench"))
            return (MuscleIds.Triceps, null);

        // Ab exercises
        if (ContainsAny(name, "crunch", "sit up", "plank", "leg raise", "ab wheel", "russian twist", "hollow hold"))
            return (MuscleIds.Abs, null);

        // Calf exercises
        if (ContainsAny(name, "calf raise", "calf press"))
            return (MuscleIds.Calves, null);

        // Lower back exercises
        if (ContainsAny(name, "hyperextension", "back extension", "superman"))
            return (MuscleIds.LowerBack, null);

        // Default fallback
        return (MuscleIds.Default, null);
    }

    /// <summary>
    ///     Helper method to check if text contains any of the provided keywords.
    /// </summary>
    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(s => text.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    // Default muscle IDs - these should match your seeded data
    private static class MuscleIds
    {
        public static readonly Guid Chest = new("00000000-0000-0000-0000-000000000002");
        public static readonly Guid Back = new("00000000-0000-0000-0000-000000000003");
        public static readonly Guid Quads = new("00000000-0000-0000-0000-000000000004");
        public static readonly Guid Hamstrings = new("00000000-0000-0000-0000-000000000005");
        public static readonly Guid Glutes = new("00000000-0000-0000-0000-000000000006");
        public static readonly Guid Shoulders = new("00000000-0000-0000-0000-000000000007");
        public static readonly Guid Biceps = new("00000000-0000-0000-0000-000000000008");
        public static readonly Guid Triceps = new("00000000-0000-0000-0000-000000000009");
        public static readonly Guid Abs = new("00000000-0000-0000-0000-000000000010");
        public static readonly Guid Calves = new("00000000-0000-0000-0000-000000000011");
        public static readonly Guid LowerBack = new("00000000-0000-0000-0000-000000000012");
        public static readonly Guid Forearms = new("00000000-0000-0000-0000-000000000013");
        public static readonly Guid Default = new("00000000-0000-0000-0000-000000000001");
    }
}