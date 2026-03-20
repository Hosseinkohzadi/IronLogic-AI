using CsvHelper.Configuration;
using IronLogic.Domain.Entities;

namespace IronLogic.Infrastructure;

public sealed class ExerciseRecordMap : ClassMap<ExerciseRecord>
{
    public ExerciseRecordMap()
    {
        Map(m => m.Date).Name("Date");
        Map(m => m.WorkoutName).Name("Workout Name");
        Map(m => m.ExerciseName).Name("Exercise Name");
        Map(m => m.SetOrder).Name("Set Order");
        Map(m => m.Weight).Name("Weight", "Weight (kg)", "Weight (lbs)", "weight_kg", "weight_lbs");
        Map(m => m.Reps).Name("Reps");
        Map(m => m.RPE).Name("RPE").Optional();
    }
}