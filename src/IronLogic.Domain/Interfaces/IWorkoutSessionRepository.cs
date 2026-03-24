using IronLogic.Domain.Entities;

namespace IronLogic.Domain.Interfaces
{
    public interface IWorkoutSessionRepository
    {
        Task<IEnumerable<WorkoutSession>> GetAllAsync();
        Task<WorkoutSession?> GetByIdAsync(Guid id);
        Task AddAsync(WorkoutSession session);
        Task<float> GetTotalVolumeAsync(int month, int year);
    }
}