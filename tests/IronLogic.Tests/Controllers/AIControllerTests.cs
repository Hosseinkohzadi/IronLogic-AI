using IronLogic.Api.Controllers;
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

        // Return an empty, non-null IReadOnlyList<ChatMessageContent>
        IReadOnlyList<ChatMessageContent> expectedContents = [];
        mockChatService
            .Setup(s => s.GetChatMessageContentsAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(expectedContents));

        // BodybuildingCoachPlugin is only used when measurement/recent weights are provided.
        var mockPlugin = new Mock<object>().Object;

        // Create controller instance
        var controller = new ChatController(
            kernel: null!,
            chatCompletionService: mockChatService.Object,
            bodybuildingPlugin: (dynamic)mockPlugin
        );

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

        // The response is an anonymous object: { answer = IReadOnlyList<ChatMessageContent>, tools = List<string> }
        ok.Value.Should().NotBeNull();
        dynamic obj = ok.Value!;
        var answer = obj.answer as IReadOnlyList<ChatMessageContent>;
        var tools = obj.tools as IReadOnlyList<string>;

        answer.Should().NotBeNull();
        answer.Should().BeEmpty(); // our mock returned an empty list

        // tools should be empty when no measurement/recent weights provided
        tools.Should().NotBeNull();
        tools.Should().BeEmpty();
    }
}