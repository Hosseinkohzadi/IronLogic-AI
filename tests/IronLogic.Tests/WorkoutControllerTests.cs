using FluentAssertions;
using IronLogic.Api.Controllers;
using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace IronLogic.Tests;

public class WorkoutControllerTests : IDisposable
{
    private readonly IWorkoutService _workoutService = Substitute.For<IWorkoutService>();
    private readonly WorkoutController _sut;

    public WorkoutControllerTests()
    {
        _sut = new WorkoutController(_workoutService);
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
    //  GetStats — Controller Action Tests
    // =====================================================================

    [Fact]
    public async Task GetStats_WhenCalled_Returns200Ok()
    {
        _workoutService.GetStatsAsync().Returns(new WorkoutStatsResponse());

        var result = await _sut.GetStats();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetStats_WhenCalled_ReturnsStatsInBody()
    {
        var stats = new WorkoutStatsResponse
        {
            TotalSessions = 10,
            TotalExercises = 30,
            TotalSets = 90,
            TotalVolume = 45000
        };

        _workoutService.GetStatsAsync().Returns(stats);

        var result = await _sut.GetStats();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = okResult.Value.Should().BeOfType<WorkoutStatsResponse>().Subject;
        body.TotalSessions.Should().Be(10);
        body.TotalExercises.Should().Be(30);
        body.TotalSets.Should().Be(90);
        body.TotalVolume.Should().Be(45000);
    }

    [Fact]
    public async Task GetStats_WhenCalled_CallsServiceExactlyOnce()
    {
        _workoutService.GetStatsAsync().Returns(new WorkoutStatsResponse());

        await _sut.GetStats();

        await _workoutService.Received(1).GetStatsAsync();
    }
}