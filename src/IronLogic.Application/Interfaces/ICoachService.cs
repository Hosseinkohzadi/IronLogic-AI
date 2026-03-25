namespace IronLogic.Application.Interfaces;

/// <summary>
///     Service contract for generating professional coaching advice from physique analytics.
/// </summary>
public interface ICoachService
{
    /// <summary>
    ///     Generates a professional, AI-style coaching advice string using physique analytics
    ///     such as a chest-to-waist (V-Taper) ratio and a monthly training volume.
    /// </summary>
    /// <param name="chestToWaistRatio">Computed chest-to-waist ratio (V-Taper). Must be greater than zero.</param>
    /// <param name="monthlyVolume">Monthly training volume in pounds.</param>
    /// <param name="userName">Optional username shown in the response. Defaults to "Athlete".</param>
    /// <returns>A task that resolves to a professional coaching advice string.</returns>
    Task<string> GenerateAdviceAsync(double chestToWaistRatio, double monthlyVolume, string userName = "Athlete");
}