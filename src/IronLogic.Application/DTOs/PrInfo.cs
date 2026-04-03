namespace IronLogic.Application.DTOs;

/// <summary>
///     Record containing PR information for an exercise.
/// </summary>
/// <param name="MaxWeight">The maximum weight achieved.</param>
/// <param name="Date">The date when the PR was achieved.</param>
public record PrInfo(decimal MaxWeight, DateTime Date);