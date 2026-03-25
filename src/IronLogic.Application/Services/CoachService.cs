using IronLogic.Application.Interfaces;

namespace IronLogic.Application.Services;

public class CoachService : ICoachService
{
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
    public async Task<string> GenerateAdviceAsync(double chestToWaistRatio, double monthlyVolume,
        string userName = "Athlete")
    {
        if (chestToWaistRatio <= 0)
            throw new ArgumentException("Chest-to-waist ratio must be greater than zero.", nameof(chestToWaistRatio));

        if (string.IsNullOrWhiteSpace(userName))
            userName = "Athlete";

        // Simulate AI processing delay
        await Task.Delay(1000);

        // V-Taper analysis
        var vTaperEvaluation = chestToWaistRatio >= 1.4
            ? "Exceptional V-Taper — your upper width relative to your waist is elite for Classic Physique."
            : chestToWaistRatio >= 1.25
                ? "Solid V-Taper — you're well positioned for classic lines but there is targeted room for improvement."
                : "V-Taper is modest — prioritize upper-back width (lats, rear delts) and waist management for classic proportions.";

        // Volume analysis
        var volumeEvaluation = monthlyVolume >= 300000
            ? "Your monthly volume is very high — recovery and joint health should be prioritized. Consider active recovery blocks."
            : monthlyVolume >= 100000
                ? "Volume is solid — this supports hypertrophy when paired with proper nutrition and sleep."
                : "Volume is on the lower side — increase weekly density or add focused accessory work to stimulate more growth.";

        // Practical, professional advice
        var advice = $"""

                      [IronLogic Coach — AI Mock]
                      Athlete: {userName}
                      V-Taper Ratio: {chestToWaistRatio:F2}
                      Monthly Volume (lbs): {monthlyVolume:N0}

                      Assessment:
                      {vTaperEvaluation}
                      {volumeEvaluation}

                      Recommendations:
                      1) Training: Add 2–3 targeted lat and upper-back sessions weekly (heavy rows, weighted pull-ups, lat-focused pulldowns) and include horizontal pull variations for thickness.
                      2) Nutrition & Recovery: Aim for 1.6–2.2 g/kg protein, maintain a modest calorie surplus for mass phases, and schedule deloads every 4–8 weeks when weekly volume is high.
                      3) Aesthetics: For Classic Physique symmetry, prioritize shoulder-to-waist contrast: reduced direct oblique work during prep and continued upper-back width emphasis.
                      4) Monitoring: Recheck measurements in 4–6 weeks and track volume trends. If you spike volume, proactively increase protein and recovery modalities (sleep, tempo, mobility).

                      Tone: Professional, actionable, and focused on sustainable progress.
                      """;

        return advice;
    }

    /// <summary>
    ///     Generates a personalized coaching advice based on the user's recent workout stats.
    ///     Currently, uses a rule-based mock engine. Can be upgraded to Semantic Kernel later.
    /// </summary>
    /// <param name="maxBench">User's max bench press in pounds.</param>
    /// <param name="monthlyVolume">Optional monthly training volume in pounds.</param>
    /// <param name="formattedTopExercises">A formatted string describing the user's top exercises.</param>
    /// <param name="userName">Optional username shown in the response. Defaults to "Athlete".</param>
    public async Task<string> AnalyzeWorkoutStatsAsync(double maxBench, double? monthlyVolume,
        string formattedTopExercises, string userName = "Athlete")
    {
        // Normalize userName to avoid empty or whitespace values
        if (string.IsNullOrWhiteSpace(userName)) userName = "Athlete";

        // Simulate the delay of an AI thinking process (1.5 seconds)
        await Task.Delay(1500);

        // Generate dynamic analysis based on the actual numbers
        var volumeAnalysis = monthlyVolume > 400000
            ? "Your training volume is absolutely massive. Pushing over 400k lbs a month shows elite discipline, perfect for maintaining lean mass during a Smart Cut."
            : "Solid volume this month. Make sure you are eating enough protein to recover from this workload.";

        var benchAnalysis = maxBench >= 225
            ? $"Hitting {maxBench} lbs on the bench is a serious milestone. Your upper body pressing strength is well above average."
            : $"Keep grinding on that bench press. {maxBench} lbs is a great foundation.";

        // Construct the final coaching response
        return $"""

                =========================================
                🤖 [SIMULATED IRONLOGIC AI COACH]
                =========================================
                Listen up, {userName}. I've analyzed your recent data.

                {volumeAnalysis}
                {benchAnalysis}

                Looking at your top movements:
                {formattedTopExercises}

                I see a heavy focus on your back and rear delts. This is exactly how you build that classic V-Taper for the Classic Physique stage. Keep the intensity high, stay consistent with your macros, and the results will speak for themselves!
                =========================================
                """;
    }
}