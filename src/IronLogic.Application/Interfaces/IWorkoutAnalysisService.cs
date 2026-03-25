using IronLogic.Domain.Entities;

namespace IronLogic.Application.Interfaces;

/// <summary>
///     Abstraction over workout/physique analysis helpers.
/// </summary>
public interface IWorkoutAnalysisService
{
    /// <summary>
    ///     Calculates the chest-to-waist ratio from a <see cref="MuscleMeasurement"/> instance.
    /// </summary>
    double CalculateChestToWaistRatio(MuscleMeasurement measurement);

    /// <summary>
    ///     Calculates the chest-to-waist ratio from raw chest and waist measurements (in centimeters).
    /// </summary>
    double CalculateChestToWaistRatio(double chestCm, double waistCm);
}