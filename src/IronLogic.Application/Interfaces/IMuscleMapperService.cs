namespace IronLogic.Application.Interfaces;

/// <summary>
///     Service for mapping exercise names to muscle groups.
/// </summary>
public interface IMuscleMapperService
{
    /// <summary>
    ///     Maps an exercise name to its primary and optional secondary muscle groups.
    /// </summary>
    /// <param name="exerciseName">The name of the exercise.</param>
    /// <returns>A tuple containing the primary muscle ID and optional secondary muscle ID.</returns>
    (Guid PrimaryMuscleId, Guid? SecondaryMuscleId) MapMuscles(string exerciseName);
}