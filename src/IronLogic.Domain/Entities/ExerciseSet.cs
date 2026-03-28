using IronLogic.Domain.Enums;

namespace IronLogic.Domain.Entities;

/// <summary>
///     Represents a specific type of exercise, like "Bench Press" or "Running".
/// </summary>
public class Exercise : BaseEntity
{
    /// <summary>
    ///     The name of the exercise (e.g., "Squat", "Deadlift").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     The primary muscle group targeted by the exercise.
    /// </summary>
    public Muscle PrimaryMuscle { get; set; }

    /// <summary>
    ///     A list of secondary muscles also engaged during the exercise.
    /// </summary>
    public List<Muscle> SecondaryMuscles { get; set; } = new();

    /// <summary>
    ///     The category of exercise, which defines the metrics to be tracked (e.g., weight and reps, duration, distance).
    /// </summary>
    public ExerciseType Type { get; set; }

    /// <summary>
    ///     How to perform the exercise, including proper form and technique. This is a string that can contain detailed
    ///     instructions or tips for executing the exercise correctly.
    /// </summary>
    public string? HowTo { get; set; }

    /// <summary>
    ///     An optional image illustrating the exercise form or equipment.
    /// </summary>
    public byte[]? Image { get; set; }

    /// <summary>
    ///     An optional link to a video demonstrating the exercise.
    /// </summary>
    public string? LinkOfVideo { get; set; }

    public List<ExerciseSession> ExerciseSessions { get; set; } = new();

}