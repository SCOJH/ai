// -----------------------------------------------------------------------
// <copyright file="MistralStructuredOutputExtensionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Mistral.StructuredOutputs;

namespace Compendium.Adapters.Mistral.Tests.StructuredOutputs;

public class MistralStructuredOutputExtensionsTests
{
    [Fact]
    public void WithStructuredOutput_SetsSchemaNameAndStrictKeys()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message>() };

        // Act
        var updated = request.WithStructuredOutput("""{"type":"object"}""", "Reply", strict: false);

        // Assert
        updated.AdditionalParameters![MistralStructuredOutputExtensions.SchemaKey].Should().Be("""{"type":"object"}""");
        updated.AdditionalParameters[MistralStructuredOutputExtensions.SchemaNameKey].Should().Be("Reply");
        updated.AdditionalParameters[MistralStructuredOutputExtensions.StrictKey].Should().Be(false);
    }

    [Fact]
    public void WithStructuredOutput_DefaultsStrictTrueAndNameResponse()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message>() };

        // Act
        var updated = request.WithStructuredOutput("""{"type":"object"}""");

        // Assert
        updated.AdditionalParameters![MistralStructuredOutputExtensions.SchemaNameKey].Should().Be("response");
        updated.AdditionalParameters[MistralStructuredOutputExtensions.StrictKey].Should().Be(true);
    }

    [Fact]
    public void WithJsonMode_AddsJsonModeFlag()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message>() };

        // Act
        var updated = request.WithJsonMode();

        // Assert
        updated.AdditionalParameters![MistralStructuredOutputExtensions.JsonModeKey].Should().Be(true);
    }

    [Fact]
    public void WithStructuredOutput_NullRequest_Throws()
    {
        // Arrange
        CompletionRequest? request = null;

        // Act
        var act = () => request!.WithStructuredOutput("{}");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithStructuredOutput_BlankSchema_Throws()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message>() };

        // Act
        var act = () => request.WithStructuredOutput("   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithJsonMode_NullRequest_Throws()
    {
        // Arrange
        CompletionRequest? request = null;

        // Act
        var act = () => request!.WithJsonMode();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
