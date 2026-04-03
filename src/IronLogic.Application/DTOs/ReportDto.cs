using System;

namespace IronLogic.Application.DTOs;

/// <summary>
/// Represents the response for workout statistics.
/// </summary>
/// <param name="TotalVolume">The total volume lifted across all workouts.</param>
/// <param name="TopExercise">The exercise with the highest volume.</param>
/// <param name="IntensityScore">A calculated score representing workout intensity.</param>
/// <param name="SessionDate">The date of the last workout session.</param>
/// <param name="Advice">AI-generated advice based on workout statistics.</param>
/// <param name="DailyWorkouts">A list of daily workout sessions.</param>
/// <param name="Streak">The current workout streak in days.</param>
public record WorkoutStatsResponseDto(
    decimal TotalVolume,
    decimal VolumeTrend,
    string TopExercise,
    decimal IntensityScore,
    decimal IntensityTrend,
    DateTime? SessionDate,
    object Advice,
    List<DailyWorkoutDto> DailyWorkouts,
    int Streak);

/// <summary>
/// Represents a single workout session.
/// </summary>
/// <param name="Id">The unique identifier for the workout session.</param>
/// <param name="Title">The title of the workout session.</param>
/// <param name="Duration">The duration of the workout session.</param>
public record WorkoutSessionDto(Guid Id, string Title, string Duration);

/// <summary>
/// Represents a response for a single workout, including all its exercises.
/// </summary>
/// <param name="Id">The unique identifier for the workout.</param>
/// <param name="Date">The date of the workout.</param>
/// <param name="Exercises">A list of exercises performed during the workout.</param>
public record WorkoutResponseDto(
    Guid Id,
    DateTime Date,
    List<ExerciseSessionDto> Exercises
);

/// <summary>
/// Represents a single set of an exercise within a workout session.
/// </summary>
/// <param name="SetIndex">The index of the set.</param>
/// <param name="SetType">The type of set (e.g., "Warm-up", "Working").</param>
/// <param name="Reps">The number of repetitions performed.</param>
/// <param name="Weight">The weight used for the set.</param>
/// <param name="DistanceKm">The distance covered in kilometers, for cardio exercises.</param>
/// <param name="DurationSeconds">The duration of the set in seconds.</param>
/// <param name="ExerciseName">The name of the exercise.</param>
public record ExerciseSessionDto(
    int SetIndex,
    string? SetType,
    int? Reps,
    decimal? Weight,
    decimal? DistanceKm,
    int? DurationSeconds,
    string ExerciseName
);

/// <summary>
/// Represents all workout sessions for a specific day.
/// </summary>
/// <param name="Date">The date of the workouts.</param>
/// <param name="WorkoutSessionDtos">A list of workout sessions for the given date.</param>
public record DailyWorkoutDto(string Date, List<WorkoutSessionDto> WorkoutSessionDtos);