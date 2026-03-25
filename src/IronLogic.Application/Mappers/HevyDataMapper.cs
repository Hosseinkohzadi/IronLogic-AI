using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Mappers;

public class HevyDataMapper : IHevyDataMapper
{
    /// <summary>
    ///     Converts a flat list of CSV records into a hierarchical domain model.
    /// </summary>
    public List<WorkoutSession> MapToSessions(IEnumerable<ExerciseRecord> flatRecords)
    {
        ArgumentNullException.ThrowIfNull(flatRecords);

        var exerciseRecords = flatRecords.ToList();

        if (!exerciseRecords.Any())
            return new List<WorkoutSession>();

        return exerciseRecords
            .Where(r => r.Date.HasValue)
            .GroupBy(record => new { record.Date!.Value, record.WorkoutName })
            .Select(sessionGroup => new WorkoutSession
            {
                Date = sessionGroup.Key.Value,
                Name = sessionGroup.Key.WorkoutName,

                Exercises = sessionGroup
                    .GroupBy(record => record.ExerciseName)
                    .Select(exerciseGroup => new WorkoutExercise
                    {
                        Name = exerciseGroup.Key,

                        Sets = exerciseGroup
                            .Select(record => new ExerciseSet
                            {
                                SetOrder = record.SetOrder,
                                Weight = record.Weight,
                                Reps = record.Reps,
                                RPE = record.RPE
                            })
                            .OrderBy(set => set.SetOrder)
                            .ToList()
                    })
                    .ToList()
            })
            .OrderByDescending(session => session.Date)
            .ToList();
    }
}