// -----------------------------------------------------------------------
// <copyright file="MercuryOptionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Mercury.Tests.Configuration;

/// <summary>
/// Unit tests for <see cref="MercuryOptions"/>.
/// </summary>
public class MercuryOptionsTests
{
    [Fact]
    public void MercuryOptions_Defaults_AreSensible()
    {
        // Arrange / Act
        var options = new MercuryOptions();

        // Assert
        options.ApiKey.Should().BeEmpty();
        options.BaseUrl.Should().Be("https://api.inceptionlabs.ai/v1");
        options.DefaultModel.Should().Be("mercury");
        options.DefaultTemperature.Should().Be(0.7f);
        options.DefaultMaxTokens.Should().Be(4096);
        options.TimeoutSeconds.Should().Be(120);
        options.RetryAttempts.Should().Be(3);
        options.EnableLogging.Should().BeFalse();
    }

    [Fact]
    public void MercuryOptions_SectionName_IsMercury()
    {
        // Assert
        MercuryOptions.SectionName.Should().Be("Mercury");
    }

    [Fact]
    public void IsValid_WithApiKey_ReturnsTrue()
    {
        // Arrange
        var options = new MercuryOptions { ApiKey = "sk-mercury-x" };

        // Act / Assert
        options.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithDefaultEmptyApiKey_ReturnsFalse()
    {
        // Arrange
        var options = new MercuryOptions();

        // Act / Assert
        options.IsValid().Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void IsValid_WithBlankApiKey_ReturnsFalse(string apiKey)
    {
        // Arrange
        var options = new MercuryOptions { ApiKey = apiKey };

        // Act / Assert
        options.IsValid().Should().BeFalse();
    }

    [Fact]
    public void MercuryOptions_WithCustomValues_RetainsThem()
    {
        // Arrange / Act
        var options = new MercuryOptions
        {
            ApiKey = "my-key",
            BaseUrl = "https://custom.inceptionlabs.ai/v2",
            DefaultModel = "mercury-coder",
            DefaultTemperature = 0.3f,
            DefaultMaxTokens = 8192,
            TimeoutSeconds = 45,
            RetryAttempts = 5,
            EnableLogging = true,
        };

        // Assert
        options.ApiKey.Should().Be("my-key");
        options.BaseUrl.Should().Be("https://custom.inceptionlabs.ai/v2");
        options.DefaultModel.Should().Be("mercury-coder");
        options.DefaultTemperature.Should().Be(0.3f);
        options.DefaultMaxTokens.Should().Be(8192);
        options.TimeoutSeconds.Should().Be(45);
        options.RetryAttempts.Should().Be(5);
        options.EnableLogging.Should().BeTrue();
    }
}
