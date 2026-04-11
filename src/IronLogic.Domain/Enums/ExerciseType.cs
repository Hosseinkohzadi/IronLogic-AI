using System.ComponentModel.DataAnnotations;

namespace IronLogic.Domain.Enums;

/// <summary>
/// Specifies the type of exercise measurement and tracking method.
/// </summary>
public enum ExerciseType
{
    [Display(Name = "Weight & Reps", Description = "Reps, Lbs")]
    WeightAndReps,

    [Display(Name = "Bodyweight Reps", Description = "Reps")]
    BodyweightReps,

    [Display(Name = "Weighted Bodyweight", Description = "Reps, +Lbs")]
    WeightedBodyweight,

    [Display(Name = "Assisted Bodyweight", Description = "Reps, -Lbs")]
    AssistedBodyweight,

    [Display(Name = "Duration", Description = "Time")]
    Duration,

    [Display(Name = "Duration & Weight", Description = "Lbs, Time")]
    DurationAndWeight,

    [Display(Name = "Distance & Duration", Description = "Time, KM")]
    DistanceAndDuration,

    [Display(Name = "Weight & Distance", Description = "Lbs, KM")]
    WeightAndDistance
}