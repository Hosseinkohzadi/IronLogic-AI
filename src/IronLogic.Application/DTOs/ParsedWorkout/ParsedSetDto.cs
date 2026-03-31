namespace IronLogic.Application.DTOs.ParsedWorkout;

/// <summary>
/// Represents a single set of an exercise, including its index, weight, reps, and optional RPE.
/// </summary>
public class ParsedSetDto
{
    /// <summary>
    /// Gets or sets the index of the set (e.g., 1 for "Set 1").
    /// </summary>
    public int SetIndex { get; set; }

    /// <summary>
    /// Gets or sets the weight used for the set, in pounds (lbs).
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Gets or sets the number of repetitions performed.
    /// </summary>
    public int Reps { get; set; }

    /// <summary>
    /// Gets or sets the optional Rate of Perceived Exertion (RPE).
    /// </summary>
    public decimal? Rpe { get; set; }
}