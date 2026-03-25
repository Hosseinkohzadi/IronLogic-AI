using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using IronLogic.Api.Controllers;
using IronLogic.Application.DTOs;
using IronLogic.Application.Interfaces;

namespace IronLogic.Tests.Unit
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
                .Setup(s => s.AnalyzeAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedAdvice);

            var controller = new CoachController(coachService: mockCoachService.Object);

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
    }
}