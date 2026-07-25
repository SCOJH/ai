// -----------------------------------------------------------------------
// <copyright file="LiteLLMOptionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.LiteLLM.Tests.Configuration;

public class LiteLLMOptionsTests
{
    [Fact]
    public void Defaults_AreProductionSafe()
    {
        // Arrange / Act
        var options = new LiteLLMOptions();

        // Assert
        LiteLLMOptions.SectionName.Should().Be("LiteLLM");
        options.BaseUrl.Should().Be("http://localhost:4000");
        options.DefaultModel.Should().Be("openai/gpt-4o-mini");
        options.DefaultEmbeddingModel.Should().Be("openai/text-embedding-3-small");
        options.DefaultTemperature.Should().BeApproximately(0.7f, 0.0001f);
        options.DefaultMaxTokens.Should().Be(4096);
        options.TimeoutSeconds.Should().Be(120);
        options.RetryAttempts.Should().Be(3);
        options.MaxEmbeddingsBatchSize.Should().Be(2048);
        options.EnableLogging.Should().BeFalse();
        options.UseStructuredOutputsByDefault.Should().BeFalse();
        options.ApiKey.Should().BeEmpty();
        options.VirtualKey.Should().BeNull();
        options.PassThroughHeaders.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void SectionName_IsStableConstant()
    {
        // The section name must not silently change because consumers bind config to it.
        LiteLLMOptions.SectionName.Should().Be("LiteLLM");
    }

    [Fact]
    public void IsValid_WithDefaultBaseUrl_ReturnsTrue()
    {
        // Arrange
        var options = new LiteLLMOptions();

        // Act & Assert — auth is optional for local self-hosted proxies.
        options.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenBaseUrlBlank_ReturnsFalse()
    {
        // Arrange
        var options = new LiteLLMOptions { BaseUrl = "   " };

        // Act & Assert
        options.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenBaseUrlNotAbsolute_ReturnsFalse()
    {
        // Arrange
        var options = new LiteLLMOptions { BaseUrl = "not-a-uri" };

        // Act & Assert
        options.IsValid().Should().BeFalse();
    }

    [Fact]
    public void PassThroughHeaders_AreCaseInsensitive()
    {
        // Arrange
        var options = new LiteLLMOptions();
        options.PassThroughHeaders["X-LiteLLM-Tags"] = "team=ops";

        // Act
        var found = options.PassThroughHeaders.TryGetValue("x-litellm-tags", out var v);

        // Assert
        found.Should().BeTrue();
        v.Should().Be("team=ops");
    }

    [Fact]
    public void GetEffectiveAuthorizationToken_ReturnsVirtualKey_WhenSet()
    {
        // Arrange
        var options = new LiteLLMOptions
        {
            ApiKey = "plain-key",
            VirtualKey = "virtual-key-value"
        };

        // Act
        var token = InvokeGetEffectiveAuthorizationToken(options);

        // Assert
        token.Should().Be("virtual-key-value");
    }

    [Fact]
    public void GetEffectiveAuthorizationToken_FallsBackToApiKey_WhenVirtualKeyMissing()
    {
        // Arrange
        var options = new LiteLLMOptions { ApiKey = "plain-key" };

        // Act
        var token = InvokeGetEffectiveAuthorizationToken(options);

        // Assert
        token.Should().Be("plain-key");
    }

    [Fact]
    public void GetEffectiveAuthorizationToken_ReturnsNull_WhenNeitherSet()
    {
        // Arrange
        var options = new LiteLLMOptions();

        // Act
        var token = InvokeGetEffectiveAuthorizationToken(options);

        // Assert
        token.Should().BeNull();
    }

    [Fact]
    public void GetEffectiveAuthorizationToken_IgnoresWhitespaceVirtualKey()
    {
        // Arrange
        var options = new LiteLLMOptions
        {
            ApiKey = "real-key",
            VirtualKey = "   "
        };

        // Act
        var token = InvokeGetEffectiveAuthorizationToken(options);

        // Assert
        token.Should().Be("real-key");
    }

    private static string? InvokeGetEffectiveAuthorizationToken(LiteLLMOptions options)
    {
        var method = typeof(LiteLLMOptions).GetMethod(
            "GetEffectiveAuthorizationToken",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        method.Should().NotBeNull();
        return (string?)method!.Invoke(options, Array.Empty<object>());
    }
}
