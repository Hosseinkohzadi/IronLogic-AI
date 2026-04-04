using CsvHelper.Configuration.Attributes;

namespace IronLogic.Infrastructure.Mapper;

/// <summary>
///     Data transfer object for exercise records imported from CSV files.
/// </summary>
public class ExerciseRecordDto
{
    /// <summary>
    ///     Gets or sets the title of the workout session.
    /// </summary>
    [Name("title")]
    public string Title { get; set; }

    /// <summary>
    ///     Gets or sets the start time of the workout session.
    /// </summary>
    [Name("start_time")]
    public DateTime StartTime { get; set; }

    /// <summary>
    ///     Gets or sets the end time of the workout session.
    /// </summary>
    [Name("end_time")]
    public DateTime EndTime { get; set; }

    /// <summary>
    ///     Gets or sets the description of the workout session.
    /// </summary>
    [Name("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the title of the exercise performed.
    /// </summary>
    [Name("exercise_title")]
    public string ExerciseTitle { get; set; }

    /// <summary>
    ///     Gets or sets the superset identifier if the exercise is part of a superset.
    /// </summary>
    [Name("superset_id")]
    public string? SupersetId { get; set; }

    /// <summary>
    ///     Gets or sets additional notes about the exercise.
    /// </summary>
    [Name("exercise_notes")]
    public string? ExerciseNotes { get; set; }

    /// <summary>
    ///     Gets or sets the index of the set within the exercise.
    /// </summary>
    [Name("set_index")]
    public int SetIndex { get; set; }

    /// <summary>
    ///     Gets or sets the type of set (e.g., warm-up, working set, drop set).
    /// </summary>
    [Name("set_type")]
    public string SetType { get; set; }

    /// <summary>
    ///     Gets or sets the weight lifted in pounds.
    /// </summary>
    [Name("weight_lbs")]
    public decimal? WeightLbs { get; set; }

    /// <summary>
    ///     Gets or sets the number of repetitions performed.
    /// </summary>
    [Name("reps")]
    public int? Reps { get; set; }

    /// <summary>
    ///     Gets or sets the distance covered in kilometers (for cardio exercises).
    /// </summary>
    [Name("distance_km")]
    public decimal? DistanceKm { get; set; }

    /// <summary>
    ///     Gets or sets the duration in seconds (for timed exercises).
    /// </summary>
    [Name("duration_seconds")]
    public int? DurationSeconds { get; set; }

    /// <summary>
    ///     Gets or sets the Rate of Perceived Exertion (RPE) for the set.
    /// </summary>
    [Name("rpe")]
    public decimal? Rpe { get; set; }
}