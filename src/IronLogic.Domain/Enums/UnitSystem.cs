namespace IronLogic.Domain.Enums;

/// <summary>
/// Defines the measurement unit system preference for international users.
/// </summary>
public enum UnitSystem
{
    /// <summary>
    /// Metric system (kilograms, centimeters, kilometers) - used globally.
    /// </summary>
    Metric = 0,

    /// <summary>
    /// Imperial system (pounds, inches, miles) - primarily used in USA, Canada, UK.
    /// </summary>
    Imperial = 1
}
