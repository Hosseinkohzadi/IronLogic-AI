namespace IronLogic.Application.DTOs;

public record WorkoutResponseDto(
    Guid Id,
    DateTime Date,
    List<ExerciseSessionDto> Exercises
);