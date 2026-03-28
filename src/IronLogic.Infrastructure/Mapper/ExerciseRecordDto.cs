using CsvHelper.Configuration.Attributes;

namespace IronLogic.Infrastructure.Mapper;

public class ExerciseRecordDto
{
    [Name("title")] public string Title { get; set; }

    [Name("start_time")] public DateTime StartTime { get; set; }

    [Name("end_time")] public DateTime EndTime { get; set; }

    [Name("description")] public string? Description { get; set; }

    [Name("exercise_title")] public string ExerciseTitle { get; set; }

    [Name("superset_id")] public string? SupersetId { get; set; }

    [Name("exercise_notes")] public string? ExerciseNotes { get; set; }

    [Name("set_index")] public int SetIndex { get; set; }

    [Name("set_type")] public string SetType { get; set; }

    [Name("weight_lbs")] public decimal? WeightLbs { get; set; }

    [Name("reps")] public int? Reps { get; set; }

    [Name("distance_km")] public decimal? DistanceKm { get; set; }

    [Name("duration_seconds")] public int? DurationSeconds { get; set; }

    [Name("rpe")] public decimal? Rpe { get; set; }
}