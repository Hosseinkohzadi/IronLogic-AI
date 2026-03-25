using IronLogic.Domain.Entities;
using Microsoft.SemanticKernel;

namespace IronLogic.Application.Services;

/// <summary>
///     Semantic Kernel plugin providing lightweight body-metrics helpers for AI-enabled workflows.
///     Functions are annotated with <see cref="KernelFunctionAttribute"/> so they can be discovered
///     by Microsoft.SemanticKernel without using SkillDefinition / SKFunction types.
/// </summary>
public class BodyMetricsPlugin
{
    /// <summary>
    ///     AnalyzeVTaper calculates the chest-to-waist ratio and classifies the result for Classic Physique-style assessment.
    /// </summary>
    /// <param name="measurement">
    ///     The <see cref="MuscleMeasurement"/> instance used for the ratio calculation. Both
    ///     <see cref="MuscleMeasurement.Chest"/> and <see cref="MuscleMeasurement.Waist"/> must be set to positive values.
    /// </param>
    /// <returns>
    ///     A short classification string:
    ///     - "Pro Level" when chest-to-waist ratio &gt; 1.4
    ///     - "Improvement Needed" otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="measurement"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <see cref="MuscleMeasurement.Waist"/> is not positive.</exception>
    /// <remarks>
    ///     This function is intended to be used as a deterministic tool by an AI orchestration layer.
    ///     It returns a concise label so that downstream prompt engineering or UI layers can surface
    ///     a clear status without needing further numeric interpretation.
    /// </remarks>
    /// <example>
    /// <![CDATA[
    /// var plugin = new BodyMetricsPlugin();
    /// var result = plugin.AnalyzeVTaper(new MuscleMeasurement { Chest = 117f, Waist = 73.5f });
    /// // result == "Pro Level"
    /// ]]>
    /// </example>
    [KernelFunction]
    public string AnalyzeVTaper(MuscleMeasurement measurement)
    {
        if (measurement is null) throw new ArgumentNullException(nameof(measurement));
        if (measurement.Waist <= 0f) throw new ArgumentException("Waist must be a positive value.", nameof(measurement));

        var ratio = measurement.Chest / measurement.Waist;

        return ratio > 1.4f ? "Pro Level" : "Improvement Needed";
    }
}