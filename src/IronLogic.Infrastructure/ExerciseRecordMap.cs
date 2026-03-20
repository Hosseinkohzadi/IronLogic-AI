using CsvHelper.Configuration;
using IronLogic.Domain.Entities;

namespace IronLogic.Infrastructure;

/// <summary>
/// CSV mapping configuration for <see cref="ExerciseRecord"/>, defining column names and format options.
/// </summary>
public sealed class ExerciseRecordMap : ClassMap<ExerciseRecord>
{
    public ExerciseRecordMap()
    {
        Map(m => m.WorkoutName).Name("title");
        Map(m => m.Date).Name("start_time")
            .TypeConverterOption.Format("d MMM yyyy, HH:mm", "dd MMM yyyy, HH:mm", "yyyy-MM-dd HH:mm:ss");
        Map(m => m.ExerciseName).Name("exercise_title");
        Map(m => m.SetOrder).Name("set_index");
        Map(m => m.Weight).Name("weight_lbs", "weight_kg").Optional();
        Map(m => m.Reps).Name("reps").Optional();
        Map(m => m.RPE).Name("rpe").Optional();
    }
}