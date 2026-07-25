// -----------------------------------------------------------------------
// <copyright file="GeminiToolCallingExtensionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Gemini.Tools;

namespace Compendium.Adapters.Gemini.Tests.Tools;

public class GeminiToolCallingExtensionsTests
{
    [Fact]
    public void WithTools_NullRequest_Throws()
    {
        // Arrange
        CompletionRequest request = null!;

        // Act
        var act = () => request.WithTools(new List<AgentTool>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithTools_NullTools_Throws()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message> { Message.User("hi") } };

        // Act
        var act = () => request.WithTools(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithTools_AttachesToolsAndChoice_ToAdditionalParameters()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message> { Message.User("hi") } };
        var tools = new List<AgentTool> { new("t", "d") };

        // Act
        var updated = request.WithTools(tools, "auto");

        // Assert
        updated.AdditionalParameters!.Should().ContainKey(GeminiToolCallingExtensions.ToolsKey)
            .WhoseValue.Should().BeSameAs(tools);
        updated.AdditionalParameters!.Should().ContainKey(GeminiToolCallingExtensions.ToolChoiceKey)
            .WhoseValue.Should().Be("auto");
    }

    [Fact]
    public void WithTools_NoToolChoice_OmitsChoiceKey()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message> { Message.User("hi") } };

        // Act
        var updated = request.WithTools(new List<AgentTool> { new("t", "d") });

        // Assert
        updated.AdditionalParameters!.Should().NotContainKey(GeminiToolCallingExtensions.ToolChoiceKey);
    }

    [Fact]
    public void WithTools_PreservesExistingAdditionalParameters()
    {
        // Arrange
        var existing = new Dictionary<string, object> { ["foo"] = "bar" };
        var request = new CompletionRequest
        {
            Model = "m",
            Messages = new List<Message> { Message.User("hi") },
            AdditionalParameters = existing
        };

        // Act
        var updated = request.WithTools(new List<AgentTool> { new("t", "d") });

        // Assert
        updated.AdditionalParameters!.Should().ContainKey("foo");
    }

    [Fact]
    public void GetToolCalls_NullResponse_Throws()
    {
        // Arrange
        CompletionResponse response = null!;

        // Act
        var act = () => response.GetToolCalls();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetToolCalls_WithInvocationsInMetadata_ReturnsThem()
    {
        // Arrange
        var invocations = new List<AgentToolInvocation>
        {
            new("t", "{}", string.Empty, false, TimeSpan.Zero)
        };
        var response = new CompletionResponse
        {
            Id = "x",
            Model = "m",
            Content = string.Empty,
            FinishReason = FinishReason.ToolCall,
            Usage = new UsageStats { PromptTokens = 0, CompletionTokens = 0 },
            Metadata = new Dictionary<string, object>
            {
                [GeminiToolCallingExtensions.ToolCallsMetadataKey] = (IReadOnlyList<AgentToolInvocation>)invocations
            }
        };

        // Act
        var calls = response.GetToolCalls();

        // Assert
        calls.Should().BeSameAs(invocations);
    }

    [Fact]
    public void GetToolCalls_WithWrongTypeInMetadata_ReturnsEmpty()
    {
        // Arrange
        var response = new CompletionResponse
        {
            Id = "x",
            Model = "m",
            Content = string.Empty,
            FinishReason = FinishReason.Stop,
            Usage = new UsageStats { PromptTokens = 0, CompletionTokens = 0 },
            Metadata = new Dictionary<string, object>
            {
                [GeminiToolCallingExtensions.ToolCallsMetadataKey] = "not-a-list"
            }
        };

        // Act
        var calls = response.GetToolCalls();

        // Assert
        calls.Should().BeEmpty();
    }

    [Fact]
    public void GetToolCalls_NoMetadata_ReturnsEmpty()
    {
        // Arrange
        var response = new CompletionResponse
        {
            Id = "x",
            Model = "m",
            Content = string.Empty,
            FinishReason = FinishReason.Stop,
            Usage = new UsageStats { PromptTokens = 0, CompletionTokens = 0 }
        };

        // Act
        var calls = response.GetToolCalls();

        // Assert
        calls.Should().BeEmpty();
    }
}
