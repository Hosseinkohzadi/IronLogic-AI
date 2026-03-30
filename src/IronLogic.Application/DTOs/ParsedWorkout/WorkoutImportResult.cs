namespace IronLogic.Application.DTOs.ParsedWorkout;

/// <summary>
/// Represents the result of a successful workout import operation.
/// </summary>
/// <param name="SessionId">The unique identifier of the newly created workout session.</param>
/// <param name="ParsedData">The structured data that was parsed from the raw text.</param>
public record WorkoutImportResult(Guid SessionId, ParsedWorkoutDto ParsedData);