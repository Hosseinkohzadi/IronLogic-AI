namespace IronLogic.Application.DTOs;

public record DayDetailsDto(
    Guid SessionId,
    string Title,
    DateTime Date,
    decimal? TotalVolume,
    List<ExerciseDetailDto> Exercises
);

public record ExerciseDetailDto(
    string ExerciseName,
    List<SetDetailDto> Sets
);

public record SetDetailDto(
    int SetIndex,
    decimal? Weight,
    int? Reps,
    decimal? Rpe
);