using System.ComponentModel.DataAnnotations;

namespace IronLogic.Application.DTOs.ParsedWorkout;

/// <summary>
/// Represents the request payload for importing a workout from raw text.
/// </summary>
public record WorkoutImportRequest
{
    /// <summary>
    /// Gets or sets the raw text content of the workout log to be imported.
    /// </summary>
    [Required(ErrorMessage = "Workout text cannot be empty.")]
    public string WorkoutText { get; set; } = string.Empty;
}