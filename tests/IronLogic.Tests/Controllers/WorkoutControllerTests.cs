using IronLogic.Api.Controllers;
using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace IronLogic.Tests.Controllers;

public class WorkoutControllerTests : IDisposable
{
    private readonly IWorkoutService _workoutService = Substitute.For<IWorkoutService>();
    private readonly IWorkoutProvider _workoutProvider = Substitute.For<IWorkoutProvider>();
    private readonly IWorkoutAnalyticsService _analyticsService = Substitute.For<IWorkoutAnalyticsService>();
    private readonly WorkoutController _sut;

    public WorkoutControllerTests()
    {
        _sut = new WorkoutController(_workoutService, _workoutProvider, _analyticsService);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    // =====================================================================
    //  GetSessions — Controller Action Tests
    // =====================================================================

    [Fact]
    public async Task GetSessions_WhenCalled_Returns200Ok()
    {
        _workoutService.GetSessionsAsync().Returns([]);

        var result = await _sut.GetSessions();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetSessions_WhenCalled_ReturnsSessionList()
    {
        var sessions = new List<WorkoutSession>
        {
            new() { Date = new DateTime(2026, 3, 20), Name = "Push Day" }
        };

        _workoutService.GetSessionsAsync().Returns(sessions);

        var result = await _sut.GetSessions();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeAssignableTo<List<WorkoutSession>>().Subject;
        body.Should().ContainSingle().Which.Name.Should().Be("Push Day");
    }

    [Fact]
    public async Task GetSessions_WhenCalled_CallsServiceExactlyOnce()
    {
        _workoutService.GetSessionsAsync().Returns([]);

        await _sut.GetSessions();

        await _workoutService.Received(1).GetSessionsAsync();
    }

    // =====================================================================
    //  GetStats — Case A: Returns 200 with correct calculations
    // =====================================================================

    [Fact]
    public async Task GetStats_WhenSessionExists_Returns200OkWithCorrectStats()
    {
        // Arrange
        var sessionDate = new DateTime(2026, 3, 25, 9, 0, 0, DateTimeKind.Utc);
        var session = new HevyWorkoutSessionDto
        {
            Title = "Push Day",
            StartTime = sessionDate,
            Exercises =
            [
                new HevyExerciseDto
                {
                    Name = "Bench Press",
                    Sets = [new HevySetDto { Weight = 100.0, Reps = 10, SetType = "work" }]
                }
            ]
        };

        _workoutProvider.GetRecentSessionsAsync(1).Returns(new List<HevyWorkoutSessionDto> { session });
        _analyticsService.CalculateTotalVolume(session).Returns(1000.0);
        _analyticsService.GetIntensityScore(session).Returns(100.0);
        _analyticsService.GetTopExercise(session).Returns(session.Exercises[0]);

        // Act
        var result = await _sut.GetStats();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var body = okResult.Value.Should().BeOfType<WorkoutStatsResponse>().Subject;
        body.TotalVolume.Should().Be(1000.0);
        body.TopExercise.Should().Be("Bench Press");
        body.IntensityScore.Should().Be(100.0);
        body.SessionDate.Should().Be(sessionDate);
    }

    [Fact]
    public async Task GetStats_WhenSessionExists_CallsAllAnalyticsServices()
    {
        // Arrange
        var session = new HevyWorkoutSessionDto
        {
            Title = "Pull Day",
            StartTime = DateTime.UtcNow,
            Exercises =
            [
                new HevyExerciseDto
                {
                    Name = "Deadlift",
                    Sets = [new HevySetDto { Weight = 180.0, Reps = 5, SetType = "work" }]
                }
            ]
        };

        _workoutProvider.GetRecentSessionsAsync(1).Returns(new List<HevyWorkoutSessionDto> { session });
        _analyticsService.CalculateTotalVolume(session).Returns(900.0);
        _analyticsService.GetIntensityScore(session).Returns(180.0);
        _analyticsService.GetTopExercise(session).Returns(session.Exercises[0]);

        // Act
        await _sut.GetStats();

        // Assert
        _analyticsService.Received(1).CalculateTotalVolume(session);
        _analyticsService.Received(1).GetIntensityScore(session);
        _analyticsService.Received(1).GetTopExercise(session);
    }

    // =====================================================================
    //  GetStats — Case B: Returns 204 when no sessions found
    // =====================================================================

    [Fact]
    public async Task GetStats_WhenNoSessions_Returns204NoContent()
    {
        // Arrange
        _workoutProvider.GetRecentSessionsAsync(1).Returns(Enumerable.Empty<HevyWorkoutSessionDto>());

        // Act
        var result = await _sut.GetStats();

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetStats_WhenNoSessions_DoesNotCallAnalyticsService()
    {
        // Arrange
        _workoutProvider.GetRecentSessionsAsync(1).Returns(Enumerable.Empty<HevyWorkoutSessionDto>());

        // Act
        await _sut.GetStats();

        // Assert
        _analyticsService.DidNotReceive().CalculateTotalVolume(Arg.Any<HevyWorkoutSessionDto>());
        _analyticsService.DidNotReceive().GetIntensityScore(Arg.Any<HevyWorkoutSessionDto>());
        _analyticsService.DidNotReceive().GetTopExercise(Arg.Any<HevyWorkoutSessionDto>());
    }

    [Fact]
    public async Task GetStats_WhenCalled_CallsProviderWithLimitOfOne()
    {
        // Arrange
        _workoutProvider.GetRecentSessionsAsync(1).Returns(Enumerable.Empty<HevyWorkoutSessionDto>());

        // Act
        await _sut.GetStats();

        // Assert
        await _workoutProvider.Received(1).GetRecentSessionsAsync(1);
    }
}