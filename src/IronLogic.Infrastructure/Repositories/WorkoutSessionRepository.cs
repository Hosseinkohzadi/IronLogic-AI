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

    public async Task<List<WorkoutResponseDto>> GetAllByUserIdAsync(Guid userId)
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

    public async Task<List<Session>> GetSessionsWithDetailsAsync(Guid userId, DateTime? startDate = null)
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
}