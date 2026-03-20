using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

public interface IHevyDataMapper
{
    List<WorkoutSession> MapToSessions(IEnumerable<ExerciseRecord> flatRecords);
}