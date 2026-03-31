using IronLogic.Domain.Constants;

namespace IronLogic.Domain.Entities;

/// <summary>
///     Represents a single workout session.
/// </summary>
public class ExerciseSession : BaseEntity
{
    /// <summary>
    ///     The order of this set within the exercise (e.g., 1st set, 2nd set).
    /// </summary>
    public int SetIndex { get; set; }

    /// <summary>
    ///     The type of set, e.g., "normal", "warmup", "dropset".
    /// </summary>
    public string? SetType { get; set; }

    /// <summary>
    ///     The number of repetitions performed. Used for strength-based exercises.
    /// </summary>
    public int? Reps { get; set; }

    /// <summary>
    ///     The weight used for the set, in the user's preferred unit (e.g., Lbs, Kg).
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    ///     The distance covered, in kilometers. Used for cardio exercises.
    /// </summary>
    public decimal? DistanceKm { get; set; }

    /// <summary>
    ///     The duration of the set, in seconds. Used for time-based exercises.
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    ///     Rate of Perceived Exertion, indicating the intensity of the set.
    /// </summary>
    public decimal? Rpe { get; set; }

    /// <summary>
    ///     Calculated property for the total volume of a strength-based set (Reps * Weight).
    /// </summary>
    public decimal Volume => new((double)(Reps * Weight)!);

    /// <summary>
    ///     Gets or sets the weight in kilograms. This property converts the value to and from the base <see cref="Weight" />
    ///     property, which is assumed to be in pounds (Lbs).
    /// </summary>
    public decimal? WeightKg
    {
        get => Weight.HasValue ? (decimal?)Math.Round(Weight.Value / (decimal)IronAiConstants.LbsToKgFactor, 2) : null;
        set => Weight = value.HasValue ? Math.Round(value.Value * (decimal)IronAiConstants.LbsToKgFactor, 2) : null;
    }

    /// <summary>
    ///     Gets the duration formatted as a "mm:ss" string.
    /// </summary>
    public string FormattedDuration => DurationSeconds.HasValue
        ? TimeSpan.FromSeconds(DurationSeconds.Value).ToString(@"mm\:ss")
        : string.Empty;

    public Guid ExerciseId { get; init; }
    public Exercise Exercise { get; init; }

    public Guid SessionId { get; init; }
    public Session Session { get; init; }
}