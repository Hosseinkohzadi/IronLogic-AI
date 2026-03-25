using IronLogic.Api.Controllers;
using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IronLogic.Tests.Controllers
{
    public class CoachControllerTests
    {
        [Fact]
        public async Task AnalyzeAsync_Returns200AndCoachAdviceResponse_WithExpectedAdvice()
        {
            // Arrange
            var expectedAdvice = "Keep grinding, Hossein!";

            var mockCoachService = new Mock<ICoachService>();
            mockCoachService
                .Setup(s => s.GenerateAdviceAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>()))
                .ReturnsAsync(expectedAdvice);

            var mockAnalysisService = new Mock<IWorkoutAnalysisService>();
            mockAnalysisService
                .Setup(a => a.CalculateChestToWaistRatio(It.IsAny<Domain.Entities.MuscleMeasurement>()))
                .Returns(1.46);

            var controller = new CoachController(
                coachService: mockCoachService.Object,
                analysisService: mockAnalysisService.Object);

            // Act
            var actionResult = await controller.AnalyzeAsync();

            // Assert
            // Ensure we received an Ok result
            actionResult.Result.Should().BeOfType<OkObjectResult>();
            var ok = actionResult.Result as OkObjectResult;
            ok!.StatusCode.Should().Be(StatusCodes.Status200OK);

            // Ensure the payload is the expected DTO with the expected advice string
            ok.Value.Should().BeOfType<CoachAdviceResponse>();
            var payload = ok.Value as CoachAdviceResponse;
            payload!.Advice.Should().Be(expectedAdvice);
        }
    }
}