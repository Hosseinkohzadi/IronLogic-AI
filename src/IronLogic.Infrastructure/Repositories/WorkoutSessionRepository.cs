using System.Globalization;
using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Repositories;

public class WorkoutSessionRepository(AppDbContext context) : IWorkoutSessionRepository
{
    public async Task<Session?> GetByIdAsync(Guid id)
    {
        return await context.Sessions
            .Include(s => s.ExerciseSessions)
            .ThenInclude(es => es.Exercise)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<WorkoutResponseDto>> GetAllByUserIdAsync(string userId)
    {
        var workouts = await context.Sessions
            .Include(s => s.ExerciseSessions)
            .ThenInclude(es => es.Exercise)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Date)
            .Select(s => new WorkoutResponseDto(
                s.Id,
                s.Date,
                s.ExerciseSessions.Select(es => new ExerciseSessionDto(
                    es.SetIndex,
                    es.SetType,
                    es.Reps,
                    es.Weight,
                    es.DistanceKm,
                    es.DurationSeconds,
                    es.Exercise.Name
                )).ToList()
            ))
            .ToListAsync();

        return workouts;
    }

    public async Task<WorkoutStatsResponseDto> GetWorkoutStatsAsync(string userId)
    {
        var sessions = await context.Sessions
            .Where(s => s.UserId == userId)
            .Include(s => s.ExerciseSessions)
            .ThenInclude(es => es.Exercise)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        if (sessions.Count == 0)
            return new WorkoutStatsResponseDto(0, null, 0, null, null, new List<DailyWorkoutDto>(), 0);

        var dailyWorkouts = sessions
            .GroupBy(s => s.Date.Date)
            .Select(g => new DailyWorkoutDto(
                g.Key.ToString("yyyy-MM-dd"),
                g.Select(s => new WorkoutSessionDto(
                    s.Id,
                    s.Title ?? "Workout",
                    FormatDuration(s.ExerciseSessions.Sum(es => es.DurationSeconds ?? 0))
                )).ToList()
            )).ToList();

        var topExercise = sessions
            .SelectMany(s => s.ExerciseSessions)
            .GroupBy(es => es.Exercise.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        var totalVolumeLast30Days = sessions
            .Where(s => s.Date >= DateTime.UtcNow.AddDays(-30))
            .Sum(s => s.ExerciseSessions.Sum(es => (es.Weight ?? 0) * (es.Reps ?? 0)));

        var averageIntensity = sessions.Count > 0
            ? sessions.Sum(s => s.ExerciseSessions.Sum(es => (es.Weight ?? 0) * (es.Reps ?? 0))) / sessions.Count
            : 0;

        var advice = topExercise != null
            ? $"Your most frequent exercise is {topExercise}. Keep up the great work and consistency!"
            : "Start logging workouts to receive personalized advice.";

        var streak = CalculateStreak(sessions.Select(s => s.Date.Date).Distinct());

        var stats = new WorkoutStatsResponseDto(
            totalVolumeLast30Days,
            topExercise,
            averageIntensity,
            sessions.First().Date,
            new { advice },
            dailyWorkouts,
            streak
        );

        return stats;
    }

    public async Task<List<Session>> GetSessionsWithDetailsAsync(string userId, DateTime? startDate = null)
    {
        var query = context.Sessions
            .Where(s => s.UserId == userId)
            .Include(s => s.ExerciseSessions)
            .ThenInclude(es => es.Exercise)
            .AsQueryable();

        if (startDate.HasValue) query = query.Where(s => s.Date >= startDate.Value);

        return await query.OrderByDescending(s => s.Date).ToListAsync();
    }

    public Task Add(Session session)
    {
        context.Sessions.Add(session);
        return Task.CompletedTask;
    }

    public void Update(Session session)
    {
        context.Sessions.Update(session);
    }

    public void Delete(Session session)
    {
        context.Sessions.Remove(session);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<object> GetWeeklyVolumeTrend(string userId, DateTime twelveWeeksAgo)
    {
        var result = await context.Sessions
            .Where(s => s.UserId == userId && s.Date >= twelveWeeksAgo)
            .SelectMany(s => s.ExerciseSessions,
                (session, exerciseSession) => new { session.Date, exerciseSession.Weight, exerciseSession.Reps })
            .GroupBy(s =>
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(s.Date, CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday))
            .Select(g => new
            {
                WeekNumber = g.Key,
                TotalVolume = g.Sum(es => (es.Weight ?? 0) * (es.Reps ?? 0)),
                WorkoutCount = g.Select(s => s.Date).Distinct().Count()
            })
            .OrderBy(x => x.WeekNumber)
            .ToListAsync();

        return result;
    }

    private static int CalculateStreak(IEnumerable<DateTime> workoutDates)
    {
        if (!workoutDates.Any()) return 0;

        var orderedDates = workoutDates.OrderByDescending(d => d).ToList();
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var streak = 0;

        // Check if the most recent workout was today or yesterday
        if (orderedDates[0] != today && orderedDates[0] != yesterday)
            return streak;

        streak = 1;
        for (var i = 0; i < orderedDates.Count - 1; i++)
            if ((orderedDates[i] - orderedDates[i + 1]).TotalDays == 1)
                streak++;
            else
                break;

        return streak;
    }

    // Helper method to format time (e.g., 1h 20m)
    private static string FormatDuration(int seconds)
    {
        if (seconds <= 0) return "Time N/A";
        var time = TimeSpan.FromSeconds(seconds);
        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours}h {time.Minutes}m"
            : $"{time.Minutes}m";
    }
}