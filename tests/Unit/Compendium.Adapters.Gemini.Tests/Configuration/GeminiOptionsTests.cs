// -----------------------------------------------------------------------
// <copyright file="GeminiOptionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Gemini.Tests.Configuration;

public class GeminiOptionsTests
{
    [Fact]
    public void Defaults_AreSensible()
    {
        // Arrange / Act
        var sut = new GeminiOptions();

        // Assert
        sut.ApiKey.Should().BeEmpty();
        sut.BaseUrl.Should().Be("https://generativelanguage.googleapis.com");
        sut.ApiVersion.Should().Be("v1beta");
        sut.DefaultModel.Should().Be("gemini-2.0-flash");
        sut.DefaultEmbeddingModel.Should().Be("text-embedding-004");
        sut.DefaultTemperature.Should().Be(0.7f);
        sut.DefaultMaxTokens.Should().Be(4096);
        sut.TimeoutSeconds.Should().Be(120);
        sut.RetryAttempts.Should().Be(3);
        sut.EnableLogging.Should().BeFalse();
        sut.UseStructuredOutputsByDefault.Should().BeFalse();
        sut.MaxEmbeddingsBatchSize.Should().Be(100);
    }

    [Fact]
    public void SectionName_IsStable()
    {
        // Stable contract — downstream apps bind on this constant.
        GeminiOptions.SectionName.Should().Be("Gemini");
    }

    [Fact]
    public void Setters_AssignThrough()
    {
        // Arrange
        var sut = new GeminiOptions();

        // Act
        sut.ApiKey = "k";
        sut.BaseUrl = "https://custom";
        sut.ApiVersion = "v2";
        sut.DefaultModel = "m";
        sut.DefaultEmbeddingModel = "em";
        sut.DefaultTemperature = 0.1f;
        sut.DefaultMaxTokens = 999;
        sut.TimeoutSeconds = 7;
        sut.RetryAttempts = 9;
        sut.EnableLogging = true;
        sut.UseStructuredOutputsByDefault = true;
        sut.MaxEmbeddingsBatchSize = 50;

        // Assert
        sut.ApiKey.Should().Be("k");
        sut.BaseUrl.Should().Be("https://custom");
        sut.ApiVersion.Should().Be("v2");
        sut.DefaultModel.Should().Be("m");
        sut.DefaultEmbeddingModel.Should().Be("em");
        sut.DefaultTemperature.Should().Be(0.1f);
        sut.DefaultMaxTokens.Should().Be(999);
        sut.TimeoutSeconds.Should().Be(7);
        sut.RetryAttempts.Should().Be(9);
        sut.EnableLogging.Should().BeTrue();
        sut.UseStructuredOutputsByDefault.Should().BeTrue();
        sut.MaxEmbeddingsBatchSize.Should().Be(50);
    }
}
