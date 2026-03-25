using IronLogic.Application.DTOs;
using IronLogic.Application.Services;

namespace IronLogic.Tests.Services;

public class WorkoutAnalyticsServiceTests
{
    private readonly WorkoutAnalyticsService _sut = new();

    [Fact]
    public void CalculateSessionVolume_ReturnsZero_WhenSessionIsNull()
    {
        // Arrange
        HevyWorkoutSessionDto? session = null;

        // Act
        var volume = _sut.CalculateSessionVolume(session!);

        // Assert
        volume.Should().Be(0.0);
    }

    [Fact]
    public void CalculateSessionVolume_SumsWeightTimesReps_AndIgnoresNullValues()
    {
        // Arrange
        var session = new HevyWorkoutSessionDto
        {
            Exercises = new List<HevyExerciseDto>
            {
                new()
                {
                    Name = "Bench",
                    Sets = new List<HevySetDto>
                    {
                        new() { Weight = 200.0, Reps = 5, SetType = "work" }, // 1000
                        new() { Weight = 0.0, Reps = 8, SetType = "warmup" }, // 0
                        new() { Weight = null, Reps = 6, SetType = "work" }, // treated as 0
                        new() { Weight = 220.0, Reps = null, SetType = "work" } // treated as 0
                    }
                },
                new()
                {
                    Name = "Row",
                    Sets = new List<HevySetDto>
                    {
                        new() { Weight = 160.0, Reps = 6, SetType = "work" } // 960
                    }
                }
            }
        };

        // Act
        var volume = _sut.CalculateSessionVolume(session);

        // Assert
        // Expected = 1000 + 960 = 1960
        volume.Should().BeApproximately(1960.0, 0.001);
    }

    [Fact]
    public void IsPersonalRecord_ReturnsTrue_WhenNoHistoryAndCurrentHasNonZeroSet()
    {
        // Arrange
        var current = new HevyExerciseDto
        {
            Name = "Back Squat",
            Sets = new List<HevySetDto>
            {
                new() { Weight = 240.0, Reps = 5, SetType = "work" } // 1200
            }
        };

        IEnumerable<HevyWorkoutSessionDto>? history = null;

        // Act
        var isPr = _sut.IsPersonalRecord(current, history ?? Array.Empty<HevyWorkoutSessionDto>());

        // Assert
        isPr.Should().BeTrue();
    }

    [Fact]
    public void IsPersonalRecord_ReturnsFalse_WhenHistoricalMaxIsHigherOrEqual()
    {
        // Arrange
        var current = new HevyExerciseDto
        {
            Name = "Deadlift",
            Sets = new List<HevySetDto>
            {
                new() { Weight = 300.0, Reps = 3, SetType = "work" } // 900
            }
        };

        var history = new List<HevyWorkoutSessionDto>
        {
            new()
            {
                Exercises = new List<HevyExerciseDto>
                {
                    new()
                    {
                        Name = "Deadlift",
                        Sets = new List<HevySetDto>
                        {
                            new() { Weight = 320.0, Reps = 3, SetType = "work" } // 960
                        }
                    }
                }
            }
        };

        // Act
        var isPr = _sut.IsPersonalRecord(current, history);

        // Assert
        isPr.Should().BeFalse();
    }

    [Fact]
    public void IsPersonalRecord_ReturnsTrue_WhenCurrentSingleSetExceedsHistoricalMax()
    {
        // Arrange
        var current = new HevyExerciseDto
        {
            Name = "Bench Press",
            Sets = new List<HevySetDto>
            {
                new() { Weight = 230.0, Reps = 5, SetType = "work" }, // 1150
                new() { Weight = 220.0, Reps = 5, SetType = "work" } // 1100
            }
        };

        var history = new List<HevyWorkoutSessionDto>
        {
            new()
            {
                Exercises = new List<HevyExerciseDto>
                {
                    new()
                    {
                        Name = "Bench Press",
                        Sets = new List<HevySetDto>
                        {
                            new() { Weight = 225.0, Reps = 5, SetType = "work" } // 1125
                        }
                    }
                }
            }
        };

        // Act
        var isPr = _sut.IsPersonalRecord(current, history);

        // Assert
        isPr.Should().BeTrue();
    }

    [Fact]
    public void IsPersonalRecord_HandlesNullWeightsAndReps_Safely()
    {
        // Arrange
        var current = new HevyExerciseDto
        {
            Name = "Pull-up",
            Sets = new List<HevySetDto>
            {
                new() { Weight = null, Reps = null, SetType = "work" }, // treated as 0
                new() { Weight = 25.0, Reps = 4, SetType = "work" } // 100
            }
        };

        var history = new List<HevyWorkoutSessionDto>
        {
            new()
            {
                Exercises = new List<HevyExerciseDto>
                {
                    new()
                    {
                        Name = "Pull-up",
                        Sets = new List<HevySetDto>
                        {
                            new() { Weight = 20.0, Reps = 5, SetType = "work" } // 100
                        }
                    }
                }
            }
        };

        // Act
        var isPr = _sut.IsPersonalRecord(current, history);

        // Assert
        // current max == historical max (100), PR requires strictly greater
        isPr.Should().BeFalse();
    }
}