namespace IronLogic.Domain.Entities;

/// <summary>
///     Represents a single muscle measurement log entry for tracking physique progress.
/// </summary>
public class MuscleMeasurement
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime Date { get; set; }

    /// <summary>
    ///     Neck circumference in cm.
    /// </summary>
    public double Neck { get; set; }

    /// <summary>
    ///     Chest circumference in cm.
    /// </summary>
    public double Chest { get; set; }

    /// <summary>
    ///     Waist circumference in cm. Crucial for Classic Physique ratio.
    /// </summary>
    public double Waist { get; set; }

    /// <summary>
    ///     Left biceps circumference in cm (optional).
    /// </summary>
    public float? BicepsLeft { get; set; }

    /// <summary>
    ///     Right biceps circumference in cm (optional).
    /// </summary>
    public float? BicepsRight { get; set; }

    /// <summary>
    ///     Left thigh circumference in cm (optional).
    /// </summary>
    public float? ThighLeft { get; set; }

    /// <summary>
    ///     Right thigh circumference in cm (optional).
    /// </summary>
    public float? ThighRight { get; set; }
}