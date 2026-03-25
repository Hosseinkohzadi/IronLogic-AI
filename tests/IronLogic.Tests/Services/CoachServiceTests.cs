using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using IronLogic.Application.Services;
using IronLogic.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;

namespace IronLogic.Tests.Services;

public class CoachServiceTests
{
    private readonly Mock<IWorkoutAnalyticsService> _mockAnalytics = new();
    private readonly Mock<IWorkoutProvider> _mockProvider = new();
    private readonly Mock<IBodyMetricsProvider> _mockBodyMetrics = new();
    private readonly Mock<IWorkoutAnalysisService> _mockAnalysis = new();
    private readonly Mock<ILogger<CoachService>> _mockLogger = new();

    private CoachService CreateSut(Kernel? kernel = null)
    {
        return new CoachService(
            kernel ?? Kernel.CreateBuilder().Build(),
            _mockAnalytics.Object,
            _mockProvider.Object,
            _mockBodyMetrics.Object,
            _mockAnalysis.Object,
            _mockLogger.Object);
    }

    // =====================================================================
    //  GenerateAdviceAsync — Rule-Based (Legacy) Tests
    // =====================================================================

    /// <summary>
    ///     Ensure that when monthly volume is low, the generated advice contains a warning about low volume.
    ///     Follows AAA: Arrange, Act, Assert.
    /// </summary>
    [Fact]
    public async Task GenerateAdviceAsync_WhenVolumeIsLow_ShouldContainVolumeWarning()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var advice = await sut.GenerateAdviceAsync(1.10, 50_000, "UnitTest");

        // Assert
        advice.Should().Contain("Volume is on the lower side",
            "the monthly volume passed is below the low-volume threshold");
    }

    [Fact]
    public async Task GenerateAdviceAsync_WhenRatioIsZero_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.GenerateAdviceAsync(0, 100_000);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chestToWaistRatio");
    }

    [Fact]
    public async Task GenerateAdviceAsync_WhenVolumeIsHigh_ShouldContainRecoveryWarning()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var advice = await sut.GenerateAdviceAsync(1.30, 350_000, "UnitTest");

        // Assert
        advice.Should().Contain("recovery and joint health",
            "the monthly volume passed is above the high-volume threshold");
    }

    // =====================================================================
    //  AnalyzeAsync — Fallback (AI service unavailable)
    // =====================================================================

    [Fact]
    public async Task AnalyzeAsync_WhenAiUnavailable_ReturnsFallbackAdvice()
    {
        // Arrange — Kernel with no AI provider will throw on InvokePromptAsync
        var session = new HevyWorkoutSessionDto
        {
            Title = "Push Day",
            Exercises =
            [
                new HevyExerciseDto
                {
                    Name = "Bench Press",
                    Sets = [new HevySetDto { Weight = 100.0, Reps = 10, SetType = "work" }]
                }
            ]
        };

        _mockProvider
            .Setup(p => p.GetRecentSessionsAsync(1))
            .ReturnsAsync(new List<HevyWorkoutSessionDto> { session });

        _mockAnalytics.Setup(a => a.CalculateTotalVolume(session)).Returns(1000.0);
        _mockAnalytics.Setup(a => a.GetIntensityScore(session)).Returns(100.0);
        _mockAnalytics.Setup(a => a.GetTopExercise(session)).Returns(session.Exercises[0]);

        var measurement = new MuscleMeasurement { Chest = 110.0, Waist = 75.0 };
        _mockBodyMetrics.Setup(b => b.GetLatestMeasurementAsync()).ReturnsAsync(measurement);
        _mockAnalysis.Setup(a => a.CalculateChestToWaistRatio(measurement)).Returns(1.47);

        var sut = CreateSut();

        // Act
        var advice = await sut.AnalyzeAsync("TestAthlete");

        // Assert — should return rule-based fallback, not throw
        advice.Should().NotBeNullOrWhiteSpace();
        advice.Should().Contain("Rule-Based Fallback");
        advice.Should().Contain("TestAthlete");
        advice.Should().Contain("Bench Press");
    }

    [Fact]
    public async Task AnalyzeAsync_WhenNoSessions_ReturnsFallbackWithDefaultStats()
    {
        // Arrange
        _mockProvider
            .Setup(p => p.GetRecentSessionsAsync(1))
            .ReturnsAsync([]);

        _mockBodyMetrics.Setup(b => b.GetLatestMeasurementAsync()).ReturnsAsync((MuscleMeasurement?)null);

        var sut = CreateSut();

        // Act
        var advice = await sut.AnalyzeAsync();

        // Assert
        advice.Should().NotBeNullOrWhiteSpace();
        advice.Should().Contain("N/A", "top exercise should be N/A when no sessions exist");
    }

    [Fact]
    public async Task AnalyzeAsync_WhenNoMeasurements_StillReturnsAdvice()
    {
        // Arrange
        var session = new HevyWorkoutSessionDto
        {
            Title = "Pull Day",
            Exercises =
            [
                new HevyExerciseDto
                {
                    Name = "Deadlift",
                    Sets = [new HevySetDto { Weight = 180.0, Reps = 5, SetType = "work" }]
                }
            ]
        };

        _mockProvider
            .Setup(p => p.GetRecentSessionsAsync(1))
            .ReturnsAsync(new List<HevyWorkoutSessionDto> { session });

        _mockAnalytics.Setup(a => a.CalculateTotalVolume(session)).Returns(900.0);
        _mockAnalytics.Setup(a => a.GetIntensityScore(session)).Returns(180.0);
        _mockAnalytics.Setup(a => a.GetTopExercise(session)).Returns(session.Exercises[0]);

        _mockBodyMetrics.Setup(b => b.GetLatestMeasurementAsync()).ReturnsAsync((MuscleMeasurement?)null);

        var sut = CreateSut();

        // Act
        var advice = await sut.AnalyzeAsync();

        // Assert
        advice.Should().NotBeNullOrWhiteSpace();
        advice.Should().Contain("Deadlift");
    }

    [Fact]
    public async Task AnalyzeAsync_CallsProviderAndBodyMetricsExactlyOnce()
    {
        // Arrange
        _mockProvider
            .Setup(p => p.GetRecentSessionsAsync(1))
            .ReturnsAsync([]);

        _mockBodyMetrics
            .Setup(b => b.GetLatestMeasurementAsync())
            .ReturnsAsync((MuscleMeasurement?)null);

        var sut = CreateSut();

        // Act
        await sut.AnalyzeAsync();

        // Assert
        _mockProvider.Verify(p => p.GetRecentSessionsAsync(1), Times.Once);
        _mockBodyMetrics.Verify(b => b.GetLatestMeasurementAsync(), Times.Once);
    }
}
