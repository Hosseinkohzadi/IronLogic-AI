using FluentAssertions;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using IronLogic.Infrastructure.Services;
using NSubstitute;

namespace IronLogic.Tests;

/// <summary>
/// Unit tests for WorkoutService.
/// Verifies repository delegation and that TotalVolume is scoped to the current calendar month.
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
    public async Task GetStatsAsync_EmptyDatabase_ReturnsZeroValues()
    {
        _repository.GetAllWithExercisesAndSetsAsync().Returns([]);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        var stats = await _sut.GetStatsAsync();

        stats.TotalSessions.Should().Be(0);
        stats.TotalExercises.Should().Be(0);
        stats.TotalSets.Should().Be(0);
        stats.TotalVolume.Should().Be(0);
    }

    [Fact]
    public async Task GetStatsAsync_WithSessions_ReturnsTotalSessionCount()
    {
        var sessions = new List<WorkoutSession>
        {
            new() { Date = new DateTime(2026, 3, 10), Name = "Push Day", Exercises = [] },
            new() { Date = new DateTime(2026, 3, 12), Name = "Pull Day", Exercises = [] }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(sessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        var stats = await _sut.GetStatsAsync();

        stats.TotalSessions.Should().Be(2);
    }

    [Fact]
    public async Task GetStatsAsync_WithExercises_ReturnsTotalExerciseCount()
    {
        var sessions = new List<WorkoutSession>
        {
            new()
            {
                Date = new DateTime(2026, 3, 10),
                Name = "Push Day",
                Exercises =
                [
                    new WorkoutExercise { Name = "Bench Press", Sets = [] },
                    new WorkoutExercise { Name = "Overhead Press", Sets = [] }
                ]
            }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(sessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        var stats = await _sut.GetStatsAsync();

        stats.TotalExercises.Should().Be(2);
    }

    [Fact]
    public async Task GetStatsAsync_WithSets_ReturnsTotalSetCount()
    {
        var sessions = new List<WorkoutSession>
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
                            new ExerciseSet { SetOrder = 1, Weight = 80, Reps = 10 },
                            new ExerciseSet { SetOrder = 2, Weight = 85, Reps = 8 },
                            new ExerciseSet { SetOrder = 3, Weight = 90, Reps = 6 }
                        ]
                    }
                ]
            }
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(sessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        var stats = await _sut.GetStatsAsync();

        stats.TotalSets.Should().Be(3);
    }

    // =====================================================================
    //  GetStatsAsync — Current Month Volume Scoping
    // =====================================================================

    [Fact]
    public async Task GetStatsAsync_VolumeOnlyIncludesCurrentMonthSessions()
    {
        // All sessions (across months) for total counts
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
        var currentMonthSessions = new List<WorkoutSession>
        {
            allSessions[1] // Only the March session
        };

        _repository.GetAllWithExercisesAndSetsAsync().Returns(allSessions);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(currentMonthSessions);

        var stats = await _sut.GetStatsAsync();

        // Total counts include all sessions
        stats.TotalSessions.Should().Be(2);
        stats.TotalExercises.Should().Be(2);
        stats.TotalSets.Should().Be(2);

        // Volume should ONLY include current month (800), NOT last month (1000)
        stats.TotalVolume.Should().Be(800);
    }

    [Fact]
    public async Task GetStatsAsync_NoCurrentMonthSessions_VolumeIsZero()
    {
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

        var stats = await _sut.GetStatsAsync();

        stats.TotalSessions.Should().Be(1);
        stats.TotalVolume.Should().Be(0, "no sessions exist in the current month");
    }

    [Fact]
    public async Task GetStatsAsync_CurrentMonthVolume_CalculatesWeightTimesReps()
    {
        // Bench: (80*10) + (85*8) + (90*6) = 800 + 680 + 540 = 2020
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
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(currentMonthSessions);

        var stats = await _sut.GetStatsAsync();

        stats.TotalVolume.Should().Be(2020);
    }

    [Fact]
    public async Task GetStatsAsync_QueriesCurrentMonthDateRange()
    {
        _repository.GetAllWithExercisesAndSetsAsync().Returns([]);
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        await _sut.GetStatsAsync();

        var now = DateTime.UtcNow;
        var expectedStart = new DateTime(now.Year, now.Month, 1);
        var expectedEnd = expectedStart.AddMonths(1).AddTicks(-1);

        await _repository.Received(1).GetByDateRangeWithExercisesAndSetsAsync(
            expectedStart,
            Arg.Is<DateTime>(d => d.Year == expectedEnd.Year && d.Month == expectedEnd.Month));
    }

    [Fact]
    public async Task GetStatsAsync_SetsWithNullWeightOrReps_TreatsAsZeroVolume()
    {
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
        _repository.GetByDateRangeWithExercisesAndSetsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(currentMonthSessions);

        var stats = await _sut.GetStatsAsync();

        stats.TotalVolume.Should().Be(0);
    }
}