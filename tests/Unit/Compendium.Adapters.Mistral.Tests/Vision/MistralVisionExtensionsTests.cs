// -----------------------------------------------------------------------
// <copyright file="MistralVisionExtensionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Mistral.Vision;

namespace Compendium.Adapters.Mistral.Tests.Vision;

public class MistralVisionExtensionsTests
{
    [Fact]
    public void WithImages_AddsImageListUnderImagesKey()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message>() };

        // Act
        var updated = request.WithImages("https://example.com/a.png", "https://example.com/b.png");

        // Assert
        var added = updated.AdditionalParameters!;
        added.Should().ContainKey(MistralVisionExtensions.ImagesKey);
        added[MistralVisionExtensions.ImagesKey]
            .Should().BeAssignableTo<IEnumerable<string>>();
        var images = (IEnumerable<string>)added[MistralVisionExtensions.ImagesKey];
        images.Should().BeEquivalentTo(new[] { "https://example.com/a.png", "https://example.com/b.png" });
    }

    [Fact]
    public void WithImages_PreservesOtherAdditionalParameters()
    {
        // Arrange
        var request = new CompletionRequest
        {
            Model = "m",
            Messages = new List<Message>(),
            AdditionalParameters = new Dictionary<string, object> { ["other"] = "kept" }
        };

        // Act
        var updated = request.WithImages("https://example.com/a.png");

        // Assert
        updated.AdditionalParameters!["other"].Should().Be("kept");
    }

    [Fact]
    public void WithImages_WithNoUrls_ReturnsUnchangedRequest()
    {
        // Arrange — calling .WithImages() with no URLs is a no-op (idiomatic for chained APIs).
        var request = new CompletionRequest { Model = "m", Messages = new List<Message>() };

        // Act
        var updated = request.WithImages();

        // Assert
        updated.Should().BeSameAs(request);
    }

    [Fact]
    public void WithImages_NullRequest_Throws()
    {
        // Arrange
        CompletionRequest? request = null;

        // Act
        var act = () => request!.WithImages("https://example.com/x.png");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithImages_NullUrlsArray_Throws()
    {
        // Arrange
        var request = new CompletionRequest { Model = "m", Messages = new List<Message>() };

        // Act
        var act = () => request.WithImages((string[])null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithImages_BlankUrl_Throws()
    {
        // Arrange — Mistral rejects blank image URLs; fail fast.
        var request = new CompletionRequest { Model = "m", Messages = new List<Message>() };

        // Act
        var act = () => request.WithImages("https://example.com/a.png", "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
