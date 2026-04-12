namespace IronLogic.Domain.Enums;

/// <summary>
/// Represents a user's primary fitness goal.
/// </summary>
public enum FitnessGoal
{
    /// <summary>
    /// No specific goal is set.
    /// </summary>
    None = 0,

    /// <summary>
    /// The user aims to lose body fat.
    /// </summary>
    FatLoss = 1,

    /// <summary>
    /// The user aims to gain muscle mass.
    /// </summary>
    MuscleGain = 2,

    /// <summary>
    /// The user aims to improve overall fitness.
    /// </summary>
    GeneralFitness = 3,

    /// <summary>
    /// The user aims to increase strength.
    /// </summary>
    Strength = 4,

    /// <summary>
    /// The user aims to improve endurance.
    /// </summary>
    Endurance = 5
}