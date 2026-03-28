using CsvHelper.Configuration;

namespace IronLogic.Infrastructure.Mapper;

public class ExerciseRecordMap : ClassMap<ExerciseRecordDto>
{
    public ExerciseRecordMap()
    {
        Map(m => m.Title).Name("title");
        Map(m => m.StartTime).Name("start_time");
        Map(m => m.EndTime).Name("end_time");
        Map(m => m.ExerciseTitle).Name("exercise_title");
        Map(m => m.SetIndex).Name("set_index");
        Map(m => m.SetType).Name("set_type");
        Map(m => m.WeightLbs).Name("weight_lbs");
        Map(m => m.Reps).Name("reps");
        Map(m => m.DistanceKm).Name("distance_km");
        Map(m => m.DurationSeconds).Name("duration_seconds");
        Map(m => m.Rpe).Name("rpe");
    }
}