using IronLogic.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IronLogic.Tests.Controllers;

public class CoachControllerTests
{
    [Fact]
    public async Task AnalyzeAsync_Returns200AndCoachAdviceResponse_WithExpectedAdvice()
    {
        // Arrange
        var expectedAdvice = "Keep grinding, Hossein!";

        var mockCoachService = new Mock<ICoachService>();
        mockCoachService
            .Setup(s => s.AnalyzeAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedAdvice);

        var controller = new CoachController(mockCoachService.Object);

        // Act
        var actionResult = await controller.AnalyzeAsync();

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var ok = actionResult.Result as OkObjectResult;
        ok!.StatusCode.Should().Be(StatusCodes.Status200OK);

        ok.Value.Should().BeOfType<CoachAdviceResponse>();
        var payload = ok.Value as CoachAdviceResponse;
        payload!.Advice.Should().Be(expectedAdvice);
    }

    [Fact]
    public async Task AnalyzeAsync_CallsCoachServiceExactlyOnce()
    {
        // Arrange
        var mockCoachService = new Mock<ICoachService>();
        mockCoachService
            .Setup(s => s.AnalyzeAsync(It.IsAny<string>()))
            .ReturnsAsync("Some advice");

        var controller = new CoachController(mockCoachService.Object);

        // Act
        await controller.AnalyzeAsync();

        // Assert
        mockCoachService.Verify(s => s.AnalyzeAsync(It.IsAny<string>()), Times.Once);
    }
}