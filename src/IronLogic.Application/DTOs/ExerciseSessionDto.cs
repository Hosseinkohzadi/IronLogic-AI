namespace IronLogic.Application.DTOs;

public record ExerciseSessionDto(
    int SetIndex,
    string? SetType,
    int? Reps,
    decimal? Weight,
    decimal? DistanceKm,
    int? DurationSeconds,
    string ExerciseName
);