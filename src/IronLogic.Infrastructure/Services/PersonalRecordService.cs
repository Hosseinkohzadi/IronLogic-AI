using IronLogic.Application.DTOs;
using IronLogic.Application.DTOs.ParsedWorkout;
using Microsoft.Extensions.Caching.Memory;

namespace IronLogic.Infrastructure.Services;

/// <summary>
///     Manages personal record tracking and caching.
/// </summary>
public class PersonalRecordService(IMemoryCache cache, AppDbContext dbContext) : IPersonalRecordService
{
    private const int PrsCacheDurationMinutes = 30;

    public async Task<Dictionary<string, PrInfo>> GetAllTimePrsAsync(string userId)
    {
        var cacheKey = GetUserPrsCacheKey(userId);

        if (cache.TryGetValue(cacheKey, out Dictionary<string, PrInfo>? allTimePrs) && allTimePrs != null)
            return allTimePrs;

        var exerciseHistory = await dbContext.ExerciseSessions
            .Where(es => es.Session.UserId == userId)
            .Select(es => new
            {
                ExerciseName = es.Exercise.Name,
                es.Weight,
                es.Session.Date
            })
            .ToListAsync();

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

    public async Task CalculatePrInsights(string userId, IEnumerable<ParsedExerciseDto> exerciseDtos,
        IReadOnlyDictionary<string, Exercise> exercises)
    {
        var allTimePrs = await GetAllTimePrsAsync(userId);

        foreach (var exerciseDto in exerciseDtos)
        {
            var currentMaxWeight = exerciseDto.Sets.Count > 0 ? exerciseDto.Sets.Max(s => s.Weight) : 0;

            if (currentMaxWeight <= 0) continue;

            var exercise = exercises[exerciseDto.Name.ToLower()];

            if (allTimePrs.TryGetValue(exercise.Name, out var prInfo))
            {
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
                exerciseDto.PrInsight = new PrInsightDto(true, currentMaxWeight, null, null);
            }
        }
    }

    public void InvalidateUserPrsCache(string userId)
    {
        cache.Remove(GetUserPrsCacheKey(userId));
    }

    private static string GetUserPrsCacheKey(string userId) => $"PRs_{userId}";
}