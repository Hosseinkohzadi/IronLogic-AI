using IronLogic.Application.DTOs;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

public interface IWorkoutSessionRepository
{
    Task<Session?> GetByIdAsync(Guid id);
    Task<List<WorkoutResponseDto>> GetAllByUserIdAsync(Guid userId);

    Task<List<Session>> GetSessionsWithDetailsAsync(Guid userId, DateTime? startDate = null);

    Task Add(Session session);
    void Update(Session session);
    void Delete(Session session);

    Task<bool> SaveChangesAsync();
}