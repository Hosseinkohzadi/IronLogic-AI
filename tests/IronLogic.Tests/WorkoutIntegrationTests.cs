using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IronLogic.Application.DTOs;
using IronLogic.Domain.Entities;
using IronLogic.Infrastructure.Data;
using IronLogic.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IronLogic.Tests;

/// <summary>
///     Integration tests for GET /api/v1/workouts/sessions and GET /api/v1/workouts/stats.
///     Uses WebApplicationFactory with an EF Core InMemory database.
///     Volume is defined as Weight * Reps, scoped to the current calendar month.
/// </summary>
public class WorkoutIntegrationTests(WebApplicationFactory factory)
    : IClassFixture<WebApplicationFactory>, IDisposable
{
    private const string SessionsEndpoint = "/api/v1/workouts/sessions";
    private const string StatsEndpoint = "/api/v1/workouts/stats";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _client = factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Seeds the InMemory database with workout data for tests that need it.
    ///     All seed data uses the current month to ensure volume tests pass.
    /// </summary>
    private async Task SeedWorkoutDataAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Sessions.Any())
            return;

        var now = DateTime.UtcNow;
        var currentMonthDate = new DateTime(now.Year, now.Month, 10);

        var session = new WorkoutSession
        {
            Date = currentMonthDate,
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
                },
                new WorkoutExercise
                {
                    Name = "Overhead Press",
                    Sets =
                    [
                        new ExerciseSet { SetOrder = 1, Weight = 40, Reps = 12 },
                        new ExerciseSet { SetOrder = 2, Weight = 45, Reps = 10 }
                    ]
                }
            ]
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();
    }

    /// <summary>
    ///     Seeds sessions across two months: one in the current month, one in the previous month.
    ///     Used to verify that volume is scoped to the current month only.
    /// </summary>
    private async Task SeedMultiMonthWorkoutDataAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Sessions.Any())
            return;

        var now = DateTime.UtcNow;
        var currentMonthDate = new DateTime(now.Year, now.Month, 10);
        var previousMonthDate = currentMonthDate.AddMonths(-1);

        var currentMonthSession = new WorkoutSession
        {
            Date = currentMonthDate,
            Name = "Current Month Push Day",
            Exercises =
            [
                new WorkoutExercise
                {
                    Name = "Bench Press",
                    Sets =
                    [
                        new ExerciseSet { SetOrder = 1, Weight = 80, Reps = 10 } // Volume = 800
                    ]
                }
            ]
        };

        var previousMonthSession = new WorkoutSession
        {
            Date = previousMonthDate,
            Name = "Previous Month Pull Day",
            Exercises =
            [
                new WorkoutExercise
                {
                    Name = "Deadlift",
                    Sets =
                    [
                        new ExerciseSet { SetOrder = 1, Weight = 140, Reps = 5 } // Volume = 700 (excluded)
                    ]
                }
            ]
        };

        db.Sessions.AddRange(currentMonthSession, previousMonthSession);
        await db.SaveChangesAsync();
    }

    // =====================================================================
    //  GET /sessions — 200 OK
    // =====================================================================

    [Fact]
    public async Task GetSessions_EmptyDatabase_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync(SessionsEndpoint, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var sessions = await response.Content.ReadFromJsonAsync<List<WorkoutSession>>(JsonOptions, CancellationToken.None);
        sessions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSessions_WithSeededData_Returns200OK()
    {
        await SeedWorkoutDataAsync();

        var response = await _client.GetAsync(SessionsEndpoint, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSessions_WithSeededData_ReturnsSessionsWithExercises()
    {
        await SeedWorkoutDataAsync();

        var response = await _client.GetAsync(SessionsEndpoint, CancellationToken.None);
        var sessions = await response.Content.ReadFromJsonAsync<List<WorkoutSession>>(JsonOptions, CancellationToken.None);

        sessions.Should().NotBeNull();
        sessions.Should().Contain(s => s.Name == "Push Day");
    }

    [Fact]
    public async Task GetSessions_WithSeededData_SessionContainsExercisesWithSets()
    {
        await SeedWorkoutDataAsync();

        var response = await _client.GetAsync(SessionsEndpoint, CancellationToken.None);
        var sessions = await response.Content.ReadFromJsonAsync<List<WorkoutSession>>(JsonOptions, CancellationToken.None);

        var pushDay = sessions!.First(s => s.Name == "Push Day");
        pushDay.Exercises.Should().HaveCountGreaterThanOrEqualTo(2);
        pushDay.Exercises.Should().Contain(e => e.Name == "Bench Press");
        pushDay.Exercises.Should().Contain(e => e.Name == "Overhead Press");
    }

    // =====================================================================
    //  GET /stats — 200 OK
    // =====================================================================

    [Fact]
    public async Task GetStats_EmptyDatabase_Returns200WithZeroValues()
    {
        var response = await _client.GetAsync(StatsEndpoint, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stats = await response.Content.ReadFromJsonAsync<WorkoutStatsResponse>(JsonOptions, CancellationToken.None);
        stats.Should().NotBeNull();
        stats.TotalVolume.Should().Be(0);
    }

    [Fact]
    public async Task GetStats_WithSeededData_Returns200OK()
    {
        await SeedWorkoutDataAsync();

        var response = await _client.GetAsync(StatsEndpoint, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStats_WithSeededData_ReturnsTotalSessions()
    {
        await SeedWorkoutDataAsync();

        var response = await _client.GetAsync(StatsEndpoint, CancellationToken.None);
        var stats = await response.Content.ReadFromJsonAsync<WorkoutStatsResponse>(JsonOptions, CancellationToken.None);

        stats.Should().NotBeNull();
        stats.TotalSessions.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetStats_WithSeededData_ReturnsTotalExercises()
    {
        await SeedWorkoutDataAsync();

        var response = await _client.GetAsync(StatsEndpoint, CancellationToken.None);
        var stats = await response.Content.ReadFromJsonAsync<WorkoutStatsResponse>(JsonOptions, CancellationToken.None);

        stats.Should().NotBeNull();
        stats.TotalExercises.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetStats_WithSeededData_ReturnsTotalSets()
    {
        await SeedWorkoutDataAsync();

        var response = await _client.GetAsync(StatsEndpoint, CancellationToken.None);
        var stats = await response.Content.ReadFromJsonAsync<WorkoutStatsResponse>(JsonOptions, CancellationToken.None);

        stats.Should().NotBeNull();
        stats.TotalSets.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetStats_WithCurrentMonthData_VolumeIsWeightTimesReps()
    {
        await SeedWorkoutDataAsync();

        var response = await _client.GetAsync(StatsEndpoint, CancellationToken.None);
        var stats = await response.Content.ReadFromJsonAsync<WorkoutStatsResponse>(JsonOptions, CancellationToken.None);

        // Expected volume from seed data (all in current month):
        // Bench: (80*10) + (85*8) + (90*6) = 800 + 680 + 540 = 2020
        // OHP:   (40*12) + (45*10)          = 480 + 450       = 930
        // Total = 2950
        stats.Should().NotBeNull();
        stats.TotalVolume.Should().BeGreaterThanOrEqualTo(2950);
    }

    // =====================================================================
    //  GET /stats — Current Month Volume Scoping
    // =====================================================================

    [Fact]
    public async Task GetStats_MultiMonthData_VolumeOnlyIncludesCurrentMonth()
    {
        await SeedMultiMonthWorkoutDataAsync();

        var response = await _client.GetAsync(StatsEndpoint, CancellationToken.None);
        var stats = await response.Content.ReadFromJsonAsync<WorkoutStatsResponse>(JsonOptions, CancellationToken.None);

        stats.Should().NotBeNull();

        // Total counts include all months
        stats.TotalSessions.Should().Be(2);

        // Volume should only include current month session: 80 * 10 = 800
        // Previous month Deadlift (140 * 5 = 700) must be excluded
        stats.TotalVolume.Should().Be(800);
    }
}