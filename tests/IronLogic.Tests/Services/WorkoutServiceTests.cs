using IronLogic.Domain.Interfaces;
using IronLogic.Infrastructure.Services;
using NSubstitute;

namespace IronLogic.Tests.Services;

/// <summary>
///     Unit tests for WorkoutService.
///     Verifies repository delegation, that TotalVolume is scoped to the current calendar month,
///     and that TopExercise, IntensityScore, and SessionDate are computed correctly.
/// </summary>
public class WorkoutServiceTests : IDisposable
{
    private readonly IWorkoutSessionRepository _repository = Substitute.For<IWorkoutSessionRepository>();
    private readonly WorkoutService _sut;

    public WorkoutServiceTests()
    {
        _sut = new WorkoutService(_repository);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    // =====================================================================
    //  GetSessionsAsync — Delegation Tests
    // =====================================================================

    [Fact]
    public async Task GetSessionsAsync_WhenCalled_DelegatesToRepository()
    {
        _repository.GetAllWithExercisesAndSetsAsync().Returns([]);

        await _sut.GetSessionsAsync();

        await _repository.Received(1).GetAllWithExercisesAndSetsAsync();
    }

    [Fact]
    public async Task GetSessionsAsync_WhenCalled_ReturnsRepositoryResult()
    {
        var sessions = new List<WorkoutSession>
        {
            new() { Date = new DateTime(2026, 3, 20), Name = "Push Day" }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(sessions);

        var result = await _sut.GetSessionsAsync();

        result.Should().BeEquivalentTo(sessions);
    }

    // =====================================================================
    //  GetStatsAsync — Aggregation Tests
    // =====================================================================

    [Fact]
    public async Task GetStatsAsync_EmptyDatabase_ReturnsDefaultValues()
    {
        // Arrange
        _repository.GetAllWithExercisesAndSetsAsync().Returns([]);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.TotalVolume.Should().Be(0);
        stats.TopExercise.Should().BeNull();
        stats.IntensityScore.Should().Be(0);
        stats.SessionDate.Should().BeNull();
    }

    // =====================================================================
    //  GetStatsAsync — TopExercise
    // =====================================================================

    [Fact]
    public async Task GetStatsAsync_WithExercises_ReturnsHighestVolumeExerciseAsTop()
    {
        // Arrange
        var currentMonthSessions = new List<WorkoutSession>
        {
            new()
            {
                Date = new DateTime(2026, 3, 10),
                Name = "Push Day",
                Exercises =
                [
                    new WorkoutExercise
                    {
                        Name = "Bench Press",
                        Sets =
                        [
                            new ExerciseSet { SetOrder = 1, Weight = 80, Reps = 10 } // Volume = 800
                        ]
                    },
                    new WorkoutExercise
                    {
                        Name = "Overhead Press",
                        Sets =
                        [
                            new ExerciseSet { SetOrder = 1, Weight = 40, Reps = 12 } // Volume = 480
                        ]
                    }
                ]
            }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(currentMonthSessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(currentMonthSessions);

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.TopExercise.Should().Be("Bench Press");
    }

    // =====================================================================
    //  GetStatsAsync — IntensityScore
    // =====================================================================

    [Fact]
    public async Task GetStatsAsync_WithSets_ReturnsCorrectIntensityScore()
    {
        // Arrange — single exercise: Volume = 80*10 = 800, Reps = 10, Intensity = 80
        var currentMonthSessions = new List<WorkoutSession>
        {
            new()
            {
                Date = new DateTime(2026, 3, 10),
                Name = "Push Day",
                Exercises =
                [
                    new WorkoutExercise
                    {
                        Name = "Bench Press",
                        Sets =
                        [
                            new ExerciseSet { SetOrder = 1, Weight = 80, Reps = 10 }
                        ]
                    }
                ]
            }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(currentMonthSessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(currentMonthSessions);

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert — 800 / 10 = 80.0
        stats.IntensityScore.Should().Be(80.0);
    }

    [Fact]
    public async Task GetStatsAsync_NoCurrentMonthReps_IntensityScoreIsZero()
    {
        // Arrange
        _repository.GetAllWithExercisesAndSetsAsync().Returns([]);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.IntensityScore.Should().Be(0);
    }

    // =====================================================================
    //  GetStatsAsync — SessionDate
    // =====================================================================

    [Fact]
    public async Task GetStatsAsync_WithSessions_ReturnsMostRecentSessionDate()
    {
        // Arrange
        var olderDate = new DateTime(2026, 2, 15);
        var newerDate = new DateTime(2026, 3, 20);

        var allSessions = new List<WorkoutSession>
        {
            new() { Date = olderDate, Name = "Old Session", Exercises = [] },
            new() { Date = newerDate, Name = "New Session", Exercises = [] }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(allSessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.SessionDate.Should().Be(newerDate);
    }

    // =====================================================================
    //  GetStatsAsync — Current Month Volume Scoping
    // =====================================================================

    [Fact]
    public async Task GetStatsAsync_VolumeOnlyIncludesCurrentMonthSessions()
    {
        // Arrange
        var allSessions = new List<WorkoutSession>
        {
            new()
            {
                Date = new DateTime(2026, 2, 15), // Last month
                Name = "Old Session",
                Exercises =
                [
                    new WorkoutExercise
                    {
                        Name = "Squat",
                        Sets = [new ExerciseSet { SetOrder = 1, Weight = 100, Reps = 10 }] // Volume = 1000
                    }
                ]
            },
            new()
            {
                Date = new DateTime(2026, 3, 10), // Current month
                Name = "Current Session",
                Exercises =
                [
                    new WorkoutExercise
                    {
                        Name = "Bench Press",
                        Sets = [new ExerciseSet { SetOrder = 1, Weight = 80, Reps = 10 }] // Volume = 800
                    }
                ]
            }
        };

        // Only current month sessions for volume calculation
        var currentMonthSessions = new List<WorkoutSession> { allSessions[1] };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(allSessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(currentMonthSessions);

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert — Volume should ONLY include current month (800), NOT last month (1000)
        stats.TotalVolume.Should().Be(800);
        stats.TopExercise.Should().Be("Bench Press");
        stats.SessionDate.Should().Be(new DateTime(2026, 3, 10));
    }

    [Fact]
    public async Task GetStatsAsync_NoCurrentMonthSessions_VolumeIsZero()
    {
        // Arrange
        var allSessions = new List<WorkoutSession>
        {
            new()
            {
                Date = new DateTime(2026, 1, 15), // Two months ago
                Name = "Old Session",
                Exercises =
                [
                    new WorkoutExercise
                    {
                        Name = "Deadlift",
                        Sets = [new ExerciseSet { SetOrder = 1, Weight = 140, Reps = 5 }] // Volume = 700
                    }
                ]
            }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(allSessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.TotalVolume.Should().Be(0, "no sessions exist in the current month");
        stats.TopExercise.Should().BeNull();
        stats.SessionDate.Should().Be(new DateTime(2026, 1, 15), "most recent session is still returned");
    }

    [Fact]
    public async Task GetStatsAsync_CurrentMonthVolume_CalculatesWeightTimesReps()
    {
        // Arrange — Bench: (80*10) + (85*8) + (90*6) = 800 + 680 + 540 = 2020
        var currentMonthSessions = new List<WorkoutSession>
        {
            new()
            {
                Date = new DateTime(2026, 3, 20),
                Name = "Push Day",
                Exercises =
                [
                    new WorkoutExercise
                    {
                        Name = "Bench Press",
                        Sets =
                        [
                            new ExerciseSet { SetOrder = 1, Weight = 80, Reps = 10 },
                            new ExerciseSet { SetOrder = 2, Weight = 85, Reps = 8 },
                            new ExerciseSet { SetOrder = 3, Weight = 90, Reps = 6 }
                        ]
                    }
                ]
            }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(currentMonthSessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(currentMonthSessions);

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.TotalVolume.Should().Be(2020);
    }

    [Fact]
    public async Task GetStatsAsync_QueriesCurrentMonthDateRange()
    {
        // Arrange
        _repository.GetAllWithExercisesAndSetsAsync().Returns([]);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        // Act
        await _sut.GetStatsAsync();

        // Assert
        var now = DateTime.UtcNow;
        var expectedStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var expectedEnd = expectedStart.AddMonths(1).AddTicks(-1);

        await _repository.Received(1).GetByDateRangeWithExercisesAndSetsAsync(
            expectedStart,
            Arg.Is<DateTime>(d => d.Year == expectedEnd.Year && d.Month == expectedEnd.Month));
    }

    [Fact]
    public async Task GetStatsAsync_SetsWithNullWeightOrReps_TreatsAsZeroVolume()
    {
        // Arrange
        var currentMonthSessions = new List<WorkoutSession>
        {
            new()
            {
                Date = new DateTime(2026, 3, 20),
                Name = "Cardio Day",
                Exercises =
                [
                    new WorkoutExercise
                    {
                        Name = "Treadmill",
                        Sets =
                        [
                            new ExerciseSet { SetOrder = 1, Weight = null, Reps = null },
                            new ExerciseSet { SetOrder = 2, Weight = 0, Reps = 10 }
                        ]
                    }
                ]
            }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(currentMonthSessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(currentMonthSessions);

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.TotalVolume.Should().Be(0);
        stats.IntensityScore.Should().Be(0);
    }
}