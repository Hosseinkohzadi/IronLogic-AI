using System;
using IronLogic.Domain.Entities;

namespace IronLogic.Application.Services;

/// <summary>
///     Provides lightweight analysis helpers for workout and physique measurements.
///     Currently focused on computing the Chest-to-Waist ratio used by bodybuilding
///     coaches (Classic Physique assessments).
/// </summary>
/// <remarks>
///     This service is intentionally dependency-free so callers can compute ratios
///     from domain entities or raw values without hitting infrastructure concerns.
///     Methods validate inputs and throw appropriate exceptions for invalid data.
/// </remarks>
public class WorkoutAnalysisService
{
    /// <summary>
    ///     Calculates the chest-to-waist ratio from a <see cref="MuscleMeasurement"/> instance.
    /// </summary>
    /// <param name="measurement">The measurement entity containing chest and waist values. Must not be <c>null</c>.</param>
    /// <returns>
    ///     A <see cref="double"/> representing the chest-to-waist ratio computed as
    ///     <c>measurement.Chest / measurement.Waist</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="measurement"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <see cref="MuscleMeasurement.Chest"/> or <see cref="MuscleMeasurement.Waist"/>
    ///     contain non-positive values (zero or negative) which would make the ratio invalid.
    /// </exception>
    public double CalculateChestToWaistRatio(MuscleMeasurement measurement)
    {
        if (measurement is null) throw new ArgumentNullException(nameof(measurement));

        return CalculateChestToWaistRatio(measurement.Chest, measurement.Waist);
    }

    /// <summary>
    ///     Calculates the chest-to-waist ratio from raw chest and waist measurements (in centimeters).
    /// </summary>
    /// <param name="chestCm">Chest circumference in centimeters. Must be greater than zero.</param>
    /// <param name="waistCm">Waist circumference in centimeters. Must be greater than zero.</param>
    /// <returns>
    ///     A <see cref="double"/> representing the chest-to-waist ratio computed as <c>chestCm / waistCm</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="chestCm"/> or <paramref name="waistCm"/> are less than or equal to zero.
    /// </exception>
    /// <remarks>
    ///     The method performs no normalization (units are assumed to be centimeters). Callers that
    ///     require a rounded result should apply rounding after receiving the value. This method returns
    ///     a full-precision <see cref="double"/> so downstream tests or business rules can assert with their
    ///     required tolerances.
    /// </remarks>
    public double CalculateChestToWaistRatio(double chestCm, double waistCm)
    {
        if (chestCm <= 0) throw new ArgumentException("Chest must be greater than zero.", nameof(chestCm));
        if (waistCm <= 0) throw new ArgumentException("Waist must be greater than zero.", nameof(waistCm));

        return chestCm / waistCm;
    }
}