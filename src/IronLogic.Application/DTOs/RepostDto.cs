namespace IronLogic.Application.DTOs;

public record WorkoutStatsResponseDto(
    decimal TotalVolume,
    string TopExercise,
    decimal IntensityScore,
    DateTime? SessionDate,
    object Advice,
    List<DailyWorkoutDto> DailyWorkouts,
    int Streak);

public record WorkoutSessionDto(Guid Id, string Title, string Duration);

public record WorkoutResponseDto(
    Guid Id,
    DateTime Date,
    List<ExerciseSessionDto> Exercises
);

public record ExerciseSessionDto(
    int SetIndex,
    string? SetType,
    int? Reps,
    decimal? Weight,
    decimal? DistanceKm,
    int? DurationSeconds,
    string ExerciseName
);

public record DailyWorkoutDto(string Date, List<WorkoutSessionDto> WorkoutSessionDtos);