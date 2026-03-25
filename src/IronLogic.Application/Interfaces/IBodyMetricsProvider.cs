namespace IronLogic.Application.Interfaces;

/// <summary>
///     Provides access to the athlete's latest body metrics (muscle measurements).
///     Used by coaching services to compute V-Taper ratios and physique analytics.
/// </summary>
public interface IBodyMetricsProvider
{
    /// <summary>
    ///     Returns the most recent <see cref="MuscleMeasurement" /> for the athlete,
    ///     or <c>null</c> if no measurements have been recorded.
    /// </summary>
    Task<MuscleMeasurement?> GetLatestMeasurementAsync();
}