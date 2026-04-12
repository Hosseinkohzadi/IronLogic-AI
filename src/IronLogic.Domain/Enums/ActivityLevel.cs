namespace IronLogic.Domain.Enums;

/// <summary>
/// Represents a user's activity level.
/// </summary>
public enum ActivityLevel
{
    /// <summary>
    /// Activity level is not specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Mostly inactive daily routine.
    /// </summary>
    Sedentary = 1,

    /// <summary>
    /// Light activity several times per week.
    /// </summary>
    LightlyActive = 2,

    /// <summary>
    /// Moderate regular activity.
    /// </summary>
    ModeratelyActive = 3,

    /// <summary>
    /// High regular activity.
    /// </summary>
    VeryActive = 4,

    /// <summary>
    /// Intense daily activity.
    /// </summary>
    ExtraActive = 5
}
