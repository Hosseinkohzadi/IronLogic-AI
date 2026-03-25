using IronLogic.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace IronLogic.Application.Services;

/// <summary>
///     Coaching service that uses Semantic Kernel for AI-driven advice generation.
///     Injects <see cref="IWorkoutAnalyticsService" /> and <see cref="IBodyMetricsProvider" />
///     to gather athlete data, then executes a prompt via <see cref="Kernel" />.
///     Falls back to deterministic rule-based advice when the AI service is unavailable.
/// </summary>
public class CoachService(
    Kernel kernel,
    IWorkoutAnalyticsService workoutAnalyticsService,
    IWorkoutProvider workoutProvider,
    IBodyMetricsProvider bodyMetricsProvider,
    IWorkoutAnalysisService workoutAnalysisService,
    ILogger<CoachService> logger) : ICoachService
{
    private const string SystemMessage =
        "You are an elite Classic Physique Coach. Analyze the provided athlete data. " +
        "Focus on V-Taper aesthetics, training intensity, and recovery.";

    private const string PromptTemplate =
        """
        {{$systemMessage}}

        Athlete Data:
        - Monthly Volume (lbs): {{$monthlyVolume}}
        - Intensity Score (avg weight per rep): {{$intensityScore}}
        - Top Exercise (by volume): {{$topExercise}}
        - V-Taper Ratio (chest/waist): {{$vTaperRatio}}
        - Waist Circumference (cm): {{$waistCircumference}}

        Provide a professional, actionable coaching analysis covering:
        1. V-Taper assessment and physique symmetry
        2. Training intensity evaluation
        3. Recovery and nutrition recommendations
        4. Specific areas for improvement

        Keep the tone professional, motivating, and data-driven.
        """;


    /// <summary>
    ///     Generates a professional, AI-style coaching advice string using physique analytics
    ///     such as a chest-to-waist (V-Taper) ratio and a monthly training volume.
    ///     This is a deterministic, rule-based mock implementation for the initial phase.
    /// </summary>
    /// <param name="chestToWaistRatio">Computed chest-to-waist ratio (V-Taper). Must be greater than zero.</param>
    /// <param name="monthlyVolume">Monthly training volume in pounds.</param>
    /// <param name="userName">Optional username shown in the response. Defaults to "Athlete".</param>
    /// <returns>
    ///     A task that resolves to a professional coaching advice string.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="chestToWaistRatio" /> is less than or equal to zero.</exception>
    public Task<string> GenerateAdviceAsync(
        double chestToWaistRatio,
        double monthlyVolume,
        string userName = "Athlete")
    {
        if (chestToWaistRatio <= 0)
            throw new ArgumentException(
                "Chest-to-waist ratio must be greater than zero.",
                nameof(chestToWaistRatio));

        if (string.IsNullOrWhiteSpace(userName))
            userName = "Athlete";

        return Task.FromResult(GenerateRuleBasedAdvice(chestToWaistRatio, monthlyVolume, userName));
    }

    /// <inheritdoc />
    public async Task<string> AnalyzeAsync(string userName = "Athlete")
    {
        if (string.IsNullOrWhiteSpace(userName))
            userName = "Athlete";

        // 1. Gather workout stats from the provider
        var recentSessions = (await workoutProvider.GetRecentSessionsAsync(1)).ToList();
        var lastSession = recentSessions.FirstOrDefault();

        var monthlyVolume = 0.0;
        var intensityScore = 0.0;
        var topExercise = "N/A";

        if (lastSession is not null)
        {
            monthlyVolume = workoutAnalyticsService.CalculateTotalVolume(lastSession);
            intensityScore = workoutAnalyticsService.GetIntensityScore(lastSession);

            var topEx = workoutAnalyticsService.GetTopExercise(lastSession);
            topExercise = topEx?.Name ?? "N/A";
        }

        // 2. Gather body metrics
        var measurement = await bodyMetricsProvider.GetLatestMeasurementAsync();

        var vTaperRatio = 0.0;
        var waistCircumference = 0.0;

        if (measurement is not null && measurement.Waist > 0)
        {
            vTaperRatio = workoutAnalysisService.CalculateChestToWaistRatio(measurement);
            waistCircumference = measurement.Waist;
        }

        // 3. Attempt Semantic Kernel prompt execution
        try
        {
            var arguments = new KernelArguments
            {
                ["systemMessage"] = SystemMessage,
                ["monthlyVolume"] = monthlyVolume.ToString("N0"),
                ["intensityScore"] = intensityScore.ToString("F1"),
                ["topExercise"] = topExercise,
                ["vTaperRatio"] = vTaperRatio.ToString("F2"),
                ["waistCircumference"] = waistCircumference.ToString("F1")
            };

            var result = await kernel.InvokePromptAsync(PromptTemplate, arguments);
            var advice = result.GetValue<string>();

            if (!string.IsNullOrWhiteSpace(advice))
                return advice;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "AI service unavailable for coaching analysis. Falling back to rule-based advice.");
        }

        // 4. Fallback: deterministic rule-based advice
        return GenerateRuleBasedAdvice(
            vTaperRatio > 0 ? vTaperRatio : 1.0,
            monthlyVolume,
            userName,
            intensityScore,
            topExercise,
            waistCircumference);
    }

    /// <summary>
    ///     Generates deterministic, rule-based coaching advice as a fallback
    ///     when the AI service is unavailable or returns an empty response.
    /// </summary>
    private static string GenerateRuleBasedAdvice(
        double chestToWaistRatio,
        double monthlyVolume,
        string userName,
        double intensityScore = 0.0,
        string topExercise = "N/A",
        double waistCircumference = 0.0)
    {
        var vTaperEvaluation = chestToWaistRatio >= 1.4
            ? "Exceptional V-Taper — your upper width relative to your waist is elite for Classic Physique."
            : chestToWaistRatio >= 1.25
                ? "Solid V-Taper — you're well positioned for classic lines but there is targeted room for improvement."
                : "V-Taper is modest — prioritize upper-back width (lats, rear delts) and waist management for classic proportions.";

        var volumeEvaluation = monthlyVolume >= 300_000
            ? "Your monthly volume is very high — recovery and joint health should be prioritized. Consider active recovery blocks."
            : monthlyVolume >= 100_000
                ? "Volume is solid — this supports hypertrophy when paired with proper nutrition and sleep."
                : "Volume is on the lower side — increase weekly density or add focused accessory work to stimulate more growth.";

        var intensityEvaluation = intensityScore >= 100
            ? "Intensity is high — ensure progressive overload is paired with adequate deload weeks."
            : intensityScore > 0
                ? "Intensity is moderate — consider increasing weight on compound movements to drive adaptation."
                : "";

        var waistNote = waistCircumference > 0
            ? $"Current waist: {waistCircumference:F1} cm. "
            : "";

        return $"""
                [IronLogic Coach — Rule-Based Fallback]
                Athlete: {userName}
                V-Taper Ratio: {chestToWaistRatio:F2}
                Monthly Volume (lbs): {monthlyVolume:N0}
                Intensity Score: {intensityScore:F1}
                Top Exercise: {topExercise}
                {waistNote}
                Assessment:
                {vTaperEvaluation}
                {volumeEvaluation}
                {intensityEvaluation}

                Recommendations:
                1) Training: Add 2–3 targeted lat and upper-back sessions weekly (heavy rows, weighted pull-ups, lat-focused pulldowns) and include horizontal pull variations for thickness.
                2) Nutrition & Recovery: Aim for 1.6–2.2 g/kg protein, maintain a modest calorie surplus for mass phases, and schedule deloads every 4–8 weeks when weekly volume is high.
                3) Aesthetics: For Classic Physique symmetry, prioritize shoulder-to-waist contrast: reduced direct oblique work during prep and continued upper-back width emphasis.
                4) Monitoring: Recheck measurements in 4–6 weeks and track volume trends. If you spike volume, proactively increase protein and recovery modalities (sleep, tempo, mobility).
                """;
    }
}