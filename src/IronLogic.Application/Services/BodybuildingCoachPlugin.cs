using System.ComponentModel;
using IronLogic.Domain.Entities;
using Microsoft.SemanticKernel;

namespace IronLogic.Application.Services;

/// <summary>
///     A small coaching "plugin" that exposes focused bodybuilding helper functions.
///     Functions are annotated for use with Microsoft.SemanticKernel and provide
///     deterministic rule-based advice for V-taper assessment and simple Smart-Cut decisions.
/// </summary>
/// <remarks>
///     Placement: Application layer service exposing domain-aware helpers that consume
///     the project's domain entities: <see cref="MuscleMeasurement"/> and <see cref="DailyWeight"/>.
/// </remarks>
public class BodybuildingCoachPlugin
{
    /// <summary>
    ///     AnalyzeVTaper calculates the chest-to-waist ratio and returns a short classification.
    /// </summary>
    /// <param name="measurement">
    ///     The <see cref="MuscleMeasurement"/> used for the ratio calculation. The <see cref="MuscleMeasurement.Chest"/>
    ///     and <see cref="MuscleMeasurement.Waist"/> values are required for a valid result.
    /// </param>
    /// <returns>
    ///     A classification string:
    ///     - "Pro Level" when chest-to-waist ratio &gt; 1.4
    ///     - "Improvement Needed" otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="measurement"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="measurement"/> has a non-positive waist value.</exception>
    /// <remarks>
    ///     This method implements the Classic Physique-style chest/waist ratio check used across the codebase and tests.
    ///     It intentionally returns a short, human-readable label so it can be easily consumed by UI layers or AI prompts.
    /// </remarks>
    /// <example>
    /// <![CDATA[
    /// var result = plugin.AnalyzeVTaper(new MuscleMeasurement { Chest = 117f, Waist = 73.5f });
    /// // result == "Pro Level"
    /// ]]>
    /// </example>
    [KernelFunction]
    [Description("Calculates chest-to-waist ratio; returns 'Pro Level' when ratio > 1.4 otherwise 'Improvement Needed'.")]
    public string AnalyzeVTaper(MuscleMeasurement measurement)
    {
        if (measurement is null) throw new ArgumentNullException(nameof(measurement));
        if (measurement.Waist <= 0f) throw new ArgumentException("Waist must be a positive value.", nameof(measurement));

        var ratio = measurement.Chest / measurement.Waist;

        return ratio > 1.4f ? "Pro Level" : "Improvement Needed";
    }

    /// <summary>
    ///     SmartCutAdvice inspects recent daily weight entries and a training "reps volume" figure
    ///     (a simple numeric representation of recent training throughput) and returns concise guidance
    ///     about whether the athlete should consider dropping calories for a cutting phase.
    /// </summary>
    /// <param name="recentWeights">
    ///     A chronological collection of <see cref="DailyWeight"/> entries spanning the observation window.
    ///     At least two entries are required to evaluate a trend.
    /// </param>
    /// <param name="repsVolume">
    ///     Numeric training volume (for example total reps*weight or aggregated daily reps-volume) for the same observation window.
    ///     This function expects a non-negative value; higher numbers indicate greater training stimulus.
    /// </param>
    /// <returns>
    ///     A short advice string. Example results:
    ///     - "Recommend dropping calories by ~5% — weight is rising while training volume is low."
    ///     - "Maintain calories — weight is stable."
    ///     - "Do not drop calories — weight is decreasing; monitor energy and recovery."
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="recentWeights"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="recentWeights"/> has fewer than 2 entries or when <paramref name="repsVolume"/> is negative.</exception>
    /// <remarks>
    ///     Implementation notes:
    ///     - Computes the percent change in bodyweight over the provided window.
    ///     - Uses the product of Weight * RepsVolume concept indirectly by evaluating whether weight is rising
    ///       without a matching high training volume. If weight is increasing and training volume is low, the advice
    ///       recommends a modest calorie reduction.
    ///     - Thresholds are intentionally conservative and deterministic so results are explainable and safe for tests.
    /// </remarks>
    /// <example>
    /// <![CDATA[
    /// var advice = plugin.SmartCutAdvice(weightsList, repsVolume: 12000);
    /// ]]>
    /// </example>
    [KernelFunction]
    [Description("Suggests whether calories should be dropped based on recent weight trend and training reps-volume.")]
    public string SmartCutAdvice(IEnumerable<DailyWeight> recentWeights, double repsVolume)
    {
        if (recentWeights is null) throw new ArgumentNullException(nameof(recentWeights));
        if (repsVolume < 0) throw new ArgumentException("Reps volume must be non-negative.", nameof(repsVolume));

        var weights = recentWeights
            .Where(w => w != null)
            .OrderBy(w => w.Date)
            .ToList();

        if (weights.Count < 2) throw new ArgumentException("At least two weight entries are required to evaluate a trend.", nameof(recentWeights));

        var first = weights.First().Weight;
        var last = weights.Last().Weight;

        if (first <= 0f) throw new ArgumentException("Historical weight values must be positive.", nameof(recentWeights));

        // Percent change in weight across window
        var weightChangePercent = (last - first) / first;

        // Simple decision rules (deterministic, conservative thresholds):
        // - If weight is rising by more than 1% and training volume is relatively low -> recommend dropping calories.
        // - If weight is rising but training volume is very high -> maintain (likely lean mass or transient).
        // - If weight is stable (within ±1%) -> maintain.
        // - If weight is falling by more than 1% -> do NOT drop calories.
        const double significantChange = 0.01; // 1%
        const double highVolumeThreshold = 20000.0; // domain-specific: treat >= 20k as high recent volume
        const double lowVolumeThreshold = 10000.0;  // below this is considered low

        if (weightChangePercent > significantChange)
        {
            if (repsVolume < lowVolumeThreshold)
            {
                return "Recommend dropping calories by ~5% — weight is trending up while training volume is low.";
            }

            if (repsVolume >= highVolumeThreshold)
            {
                return "Maintain calories and monitor for 1-2 weeks — weight is up but training volume is very high (possible muscle/neuromuscular gains).";
            }

            // Mid-volume: be cautious
            return "Consider a small calorie reduction (~2-4%) and monitor weight for 1-2 weeks.";
        }

        if (weightChangePercent < -significantChange)
        {
            return "Do not drop calories — weight is decreasing. Ensure recovery and adequate protein; consider small increase if energy is low.";
        }

        return "Maintain current calories — weight is stable. Continue monitoring weight and training volume.";
    }
}