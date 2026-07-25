// -----------------------------------------------------------------------
// <copyright file="GeminiStructuredOutputExtensionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Gemini.StructuredOutputs;

namespace Compendium.Adapters.Gemini.Tests.StructuredOutputs;

public class GeminiStructuredOutputExtensionsTests
{
    [Fact]
    public void WithStructuredOutput_NullRequest_Throws()
    {
        // Arrange
        CompletionRequest request = null!;

        // Act
        var act = () => request.WithStructuredOutput("""{"type":"object"}""");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithStructuredOutput_BadSchema_Throws(string? schema)
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message> { Message.User("h") } };

        // Act
        var act = () => request.WithStructuredOutput(schema!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithStructuredOutput_AttachesSchemaKey()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message> { Message.User("h") } };

        // Act
        var updated = request.WithStructuredOutput("""{"type":"object"}""");

        // Assert
        updated.AdditionalParameters!.Should()
            .ContainKey(GeminiStructuredOutputExtensions.SchemaKey)
            .WhoseValue.Should().Be("""{"type":"object"}""");
    }

    [Fact]
    public void WithStructuredOutput_PreservesExistingParameters()
    {
        // Arrange
        var existing = new Dictionary<string, object> { ["foo"] = 42 };
        var request = new CompletionRequest
        {
            Model = "m",
            Messages = new List<Message> { Message.User("h") },
            AdditionalParameters = existing
        };

        // Act
        var updated = request.WithStructuredOutput("""{"type":"object"}""");

        // Assert
        updated.AdditionalParameters!.Should().ContainKey("foo").WhoseValue.Should().Be(42);
    }

    [Fact]
    public void WithJsonMode_NullRequest_Throws()
    {
        // Arrange
        CompletionRequest request = null!;

        // Act
        var act = () => request.WithJsonMode();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithJsonMode_AttachesFlag()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message> { Message.User("h") } };

        // Act
        var updated = request.WithJsonMode();

        // Assert
        updated.AdditionalParameters!.Should()
            .ContainKey(GeminiStructuredOutputExtensions.JsonModeKey)
            .WhoseValue.Should().Be(true);
    }
}
