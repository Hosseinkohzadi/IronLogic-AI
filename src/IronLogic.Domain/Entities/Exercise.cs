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
    ///     Gets or sets the identifier for the primary muscle group.
    /// </summary>
    public Guid PrimaryMuscleId { get; set; }

    /// <summary>
    ///     The primary muscle group targeted by the exercise.
    /// </summary>
    public Muscle? PrimaryMuscle { get; set; }

    /// <summary>
    ///     A list of secondary muscles also engaged during the exercise.
    /// </summary>
    public List<Muscle>? SecondaryMuscles { get; set; }

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

    /// <summary>
    ///     Gets or sets the collection of exercise sessions associated with this exercise.
    /// </summary>
    public List<ExerciseSession> ExerciseSessions { get; set; } = new();

    /// <summary>
    ///     Gets or sets the equipment required for this exercise.
    /// </summary>
    public Equipment Equipment { get; set; }

    /// <summary>
    ///     Gets or sets the identifier for the required equipment.
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    ///     Gets or sets the file path to the exercise image.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    ///     Gets or sets the URL reference for additional exercise information.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Gets or sets the mechanics type of the exercise (e.g., compound, isolation).
    /// </summary>
    public string Mechanics { get; set; }

    /// <summary>
    ///     Gets or sets the detailed instructions for performing the exercise.
    /// </summary>
    public string Instructions { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the exercise image hosted externally.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user who created this exercise.
    /// </summary>
    public string CreatorUserId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the navigation property to the creator user.
    /// </summary>
    public User? CreatorUser { get; set; }

    /// <summary>
    ///     Gets or sets the approval status of the exercise.
    /// </summary>
    public ExerciseStatus Status { get; set; } = ExerciseStatus.Private;

    /// <summary>
    ///     Gets or sets whether this exercise is globally visible to all users (true only if approved by admin).
    /// </summary>
    public bool IsGlobal { get; set; }
}