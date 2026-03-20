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
        var records = flatRecords.ToList();
        if (records.Count == 0)
            return [];

        return records
            .GroupBy(record => new { record.Date, record.WorkoutName })
            .Select(sessionGroup => new WorkoutSession
            {
                Date = sessionGroup.Key.Date,
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