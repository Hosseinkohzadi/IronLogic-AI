using IronLogic.Application.DTOs;
using IronLogic.Application.Services;

namespace IronLogic.Tests.Services;

public class WorkoutAnalyticsServiceTests
{
    private readonly WorkoutAnalyticsService _sut = new();

    // =====================================================================
    //  CalculateTotalVolume
    // =====================================================================

    [Fact]
    public void CalculateTotalVolume_ReturnsZero_WhenSessionIsNull()
    {
        // Arrange
        HevyWorkoutSessionDto? session = null;

        // Act
        var volume = _sut.CalculateTotalVolume(session!);

        // Assert
        volume.Should().Be(0.0);
    }

    [Fact]
    public void CalculateTotalVolume_Returns1000_ForSingleSet100kgTimes10Reps()
    {
        // Arrange
        var session = new HevyWorkoutSessionDto
        {
            Exercises =
            [
                new HevyExerciseDto
                {
                    Name = "Squat",
                    Sets = [new HevySetDto { Weight = 100.0, Reps = 10, SetType = "work" }]
                }
            ]
        };

        // Act
        var volume = _sut.CalculateTotalVolume(session);

        // Assert
        volume.Should().Be(1000.0);
    }

    [Fact]
    public void CalculateTotalVolume_SumsWeightTimesReps_AndIgnoresNullValues()
    {
        // Arrange
        var session = new HevyWorkoutSessionDto
        {
            Exercises =
            [
                new HevyExerciseDto
                {
                    Name = "Bench",
                    Sets =
                    [
                        new HevySetDto { Weight = 200.0, Reps = 5, SetType = "work" }, // 1000
                        new HevySetDto { Weight = 0.0, Reps = 8, SetType = "warmup" }, // 0
                        new HevySetDto { Weight = null, Reps = 6, SetType = "work" }, // treated as 0
                        new HevySetDto { Weight = 220.0, Reps = null, SetType = "work" }
                    ]
                },

                new HevyExerciseDto
                {
                    Name = "Row",
                    Sets = [new HevySetDto { Weight = 160.0, Reps = 6, SetType = "work" }]
                }
            ]
        };

        // Act
        var volume = _sut.CalculateTotalVolume(session);

        // Assert  (1000 + 960 = 1960)
        volume.Should().BeApproximately(1960.0, 0.001);
    }

    // =====================================================================
    //  CalculateTotalReps
    // =====================================================================

    [Fact]
    public void CalculateTotalReps_ReturnsZero_WhenSessionIsNull()
    {
        // Arrange / Act
        var reps = _sut.CalculateTotalReps(null!);

        // Assert
        reps.Should().Be(0);
    }

    [Fact]
    public void CalculateTotalReps_SumsAllReps_AndTreatsNullAsZero()
    {
        // Arrange
        var session = new HevyWorkoutSessionDto
        {
            Exercises =
            [
                new HevyExerciseDto
                {
                    Name = "Bench",
                    Sets =
                    [
                        new HevySetDto { Weight = 100.0, Reps = 10, SetType = "work" },
                        new HevySetDto { Weight = 80.0, Reps = null, SetType = "work" }, // null → 0
                        new HevySetDto { Weight = 60.0, Reps = 8, SetType = "work" }
                    ]
                },

                new HevyExerciseDto
                {
                    Name = "Row",
                    Sets = [new HevySetDto { Weight = 70.0, Reps = 12, SetType = "work" }]
                }
            ]
        };

        // Act
        var reps = _sut.CalculateTotalReps(session);

        // Assert  (10 + 0 + 8 + 12 = 30)
        reps.Should().Be(30);
    }

    // =====================================================================
    //  CalculateVolumePerExercise
    // =====================================================================

    [Fact]
    public void CalculateVolumePerExercise_ReturnsEmptyDictionary_WhenSessionIsNull()
    {
        // Arrange / Act
        var result = _sut.CalculateVolumePerExercise(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CalculateVolumePerExercise_ReturnsCorrectBreakdown()
    {
        // Arrange
        var session = new HevyWorkoutSessionDto
        {
            Exercises =
            [
                new HevyExerciseDto
                {
                    Name = "Bench Press",
                    Sets =
                    [
                        new HevySetDto { Weight = 100.0, Reps = 10, SetType = "work" }, // 1000
                        new HevySetDto { Weight = 100.0, Reps = 8, SetType = "work" }
                    ]
                },

                new HevyExerciseDto
                {
                    Name = "Overhead Press",
                    Sets = [new HevySetDto { Weight = 60.0, Reps = 10, SetType = "work" }]
                }
            ]
        };

        // Act
        var result = _sut.CalculateVolumePerExercise(session);

        // Assert
        result.Should().HaveCount(2);
        result["Bench Press"].Should().BeApproximately(1800.0, 0.001);
        result["Overhead Press"].Should().BeApproximately(600.0, 0.001);
    }

    [Fact]
    public void CalculateVolumePerExercise_AggregatesDuplicateExerciseNames_CaseInsensitive()
    {
        // Arrange — same exercise listed twice with different casing
        var session = new HevyWorkoutSessionDto
        {
            Exercises =
            [
                new HevyExerciseDto
                {
                    Name = "Bench Press",
                    Sets = [new HevySetDto { Weight = 100.0, Reps = 5, SetType = "work" }]
                },

                new HevyExerciseDto
                {
                    Name = "bench press",
                    Sets = [new HevySetDto { Weight = 100.0, Reps = 5, SetType = "work" }]
                }
            ]
        };

        // Act
        var result = _sut.CalculateVolumePerExercise(session);

        // Assert — merged into a single key
        result.Should().HaveCount(1);
        result["Bench Press"].Should().BeApproximately(1000.0, 0.001);
    }

    // =====================================================================
        //  IsPersonalRecord
    // =====================================================================

    [Fact]
    public void IsPersonalRecord_ReturnsTrue_WhenNoHistoryAndCurrentHasNonZeroSet()
    {
        // Arrange
        var current = new HevyExerciseDto
        {
            Name = "Back Squat",
            Sets = [new HevySetDto { Weight = 240.0, Reps = 5, SetType = "work" }]
        };

        IEnumerable<HevyWorkoutSessionDto>? history = null;

        // Act
        var isPr = _sut.IsPersonalRecord(current, history ?? []);

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
            Sets = [new HevySetDto { Weight = 300.0, Reps = 3, SetType = "work" }]
        };

        var history = new List<HevyWorkoutSessionDto>
        {
            new()
            {
                Exercises =
                [
                    new HevyExerciseDto
                    {
                        Name = "Deadlift",
                        Sets = [new HevySetDto { Weight = 320.0, Reps = 3, SetType = "work" }]
                    }
                ]
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
            Sets =
            [
                new HevySetDto { Weight = 230.0, Reps = 5, SetType = "work" }, // 1150
                new HevySetDto { Weight = 220.0, Reps = 5, SetType = "work" }
            ]
        };

        var history = new List<HevyWorkoutSessionDto>
        {
            new()
            {
                Exercises =
                [
                    new HevyExerciseDto
                    {
                        Name = "Bench Press",
                        Sets = [new HevySetDto { Weight = 225.0, Reps = 5, SetType = "work" }]
                    }
                ]
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
            Sets =
            [
                new HevySetDto { Weight = null, Reps = null, SetType = "work" },
                new HevySetDto { Weight = 25.0, Reps = 4, SetType = "work" }
            ]
        };

        var history = new List<HevyWorkoutSessionDto>
        {
            new()
            {
                Exercises =
                [
                    new HevyExerciseDto
                    {
                        Name = "Pull-up",
                        Sets = [new HevySetDto { Weight = 20.0, Reps = 5, SetType = "work" }]
                    }
                ]
            }
        };

        // Act
        var isPr = _sut.IsPersonalRecord(current, history);

        // Assert — equal, not strictly greater → not a PR
        isPr.Should().BeFalse();
    }
}