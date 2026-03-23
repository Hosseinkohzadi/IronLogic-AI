namespace IronLogic.Application.Services;

public class IronLogicCoachService
{
    /// <summary>
    ///     Generates a personalized coaching advice based on the user's recent workout stats.
    ///     Currently, uses a rule-based mock engine. Can be upgraded to Semantic Kernel later.
    /// </summary>
    /// <param name="maxBench">User's max bench press in pounds.</param>
    /// <param name="monthlyVolume">Optional monthly training volume in pounds.</param>
    /// <param name="formattedTopExercises">A formatted string describing the user's top exercises.</param>
    /// <param name="userName">Optional username shown in the response. Defaults to "Athlete".</param>
    public async Task<string> AnalyzeWorkoutStatsAsync(double maxBench, double? monthlyVolume, string formattedTopExercises, string userName = "Athlete")
    {
        // Normalize userName to avoid empty or whitespace values
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = "Athlete";
        }

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