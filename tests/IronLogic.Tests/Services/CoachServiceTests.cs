using IronLogic.Application.Interfaces;
using IronLogic.Application.Services;
using IronLogic.Domain.Entities;
using Moq;

namespace IronLogic.Tests.Services;

public class CoachServiceTests
{
    /// <summary>
    ///     Ensure that when monthly volume is low, the generated advice contains a warning about low volume.
    ///     Follows AAA: Arrange, Act, Assert.
    /// </summary>
    [Fact]
    public async Task GenerateAdviceAsync_WhenVolumeIsLow_ShouldContainVolumeWarning()
    {
        // Arrange
        var mockAnalysis = new Mock<IWorkoutAnalysisService>();
        // Mock returns a plausible chest-to-waist ratio (not critical for this assertion)
        mockAnalysis.Setup(m => m.CalculateChestToWaistRatio(It.IsAny<MuscleMeasurement>())).Returns(1.10);

        var vTaper = mockAnalysis.Object.CalculateChestToWaistRatio(new MuscleMeasurement { Chest = 100, Waist = 90 });
        double lowMonthlyVolume = 50_000; // below the 100k threshold in the service

        ICoachService coachService = new CoachService();

        // Act
        var advice = await coachService.GenerateAdviceAsync(vTaper, lowMonthlyVolume, "UnitTest");

        // Assert
        advice.Should().Contain("Volume is on the lower side",
            "the monthly volume passed is below the low-volume threshold");
    }
}
