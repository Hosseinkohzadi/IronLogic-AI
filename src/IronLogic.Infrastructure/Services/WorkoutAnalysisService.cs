using IronLogic.Infrastructure.Data;

namespace IronLogic.Infrastructure.Services;

public class WorkoutAnalysisService(AppDbContext dbContext)
{
    public double GetMaxWeightForExercise(string exerciseName)
    {
        var maxWeight = dbContext.Exercises
            .Where(e => e.Name == exerciseName)
            .SelectMany(e => e.Sets)
            .Max(s => (double?)s.Weight) ?? 0;

        return maxWeight;
    }

    public double? GetTotalVolumeInDateRange(DateTime startDate, DateTime endDate)
    {
        var totalVolume = dbContext.Sessions
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .SelectMany(s => s.Exercises)
            .SelectMany(e => e.Sets)
            .Sum(s => s.Weight * s.Reps); // Volume = Weight * Reps

        return totalVolume;
    }

    public List<string> GetTopExercises(int count = 5)
    {
        return dbContext.Exercises
            .GroupBy(e => e.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(count)
            .Select(x => $"{x.Name} ({x.Count} times)")
            .ToList();
    }
}