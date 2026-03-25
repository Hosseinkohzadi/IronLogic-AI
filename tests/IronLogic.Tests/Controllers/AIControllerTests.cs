using IronLogic.Api.Controllers;
using IronLogic.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;

namespace IronLogic.Tests.Controllers;

public class AIControllerTests
{
    [Fact]
    public async Task Ask_Returns200Ok_WithAnswerAndEmptyTools_WhenPromptOnly()
    {
        // Arrange
        var mockChatService = new Mock<IChatCompletionService>();

        // Mock the actual interface method, not the extension method.
        // The extension method GetChatMessageContentsAsync(string) internally calls this.
        IReadOnlyList<ChatMessageContent> expectedContents = [];
        mockChatService
            .Setup(s => s.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContents);

        // BodybuildingCoachPlugin is a concrete class — use a real instance.
        // It is only invoked when measurement/recent weights are provided.
        var plugin = new BodybuildingCoachPlugin();

        var controller = new ChatController(
            kernel: null!,
            chatCompletionService: mockChatService.Object,
            bodybuildingPlugin: plugin);

        var request = new ChatController.AskRequest
        {
            Prompt = "Hello AI"
        };

        // Act
        var result = await controller.Ask(request);

        // Assert: 200 OK
        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.StatusCode.Should().Be(200);

        // The response is an anonymous object from another assembly.
        // Anonymous types are internal, so dynamic binding fails across assemblies.
        // Use reflection to read the properties instead.
        ok.Value.Should().NotBeNull();
        var responseType = ok.Value!.GetType();

        var answer = responseType.GetProperty("answer")?.GetValue(ok.Value) as IReadOnlyList<ChatMessageContent>;
        var tools = responseType.GetProperty("tools")?.GetValue(ok.Value) as IReadOnlyList<string>;

        answer.Should().NotBeNull();
        answer.Should().BeEmpty(); // our mock returned an empty list

        // tools should be empty when no measurement/recent weights provided
        tools.Should().NotBeNull();
        tools.Should().BeEmpty();
    }
}