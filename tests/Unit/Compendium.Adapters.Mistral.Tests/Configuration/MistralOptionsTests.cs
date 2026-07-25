// -----------------------------------------------------------------------
// <copyright file="MistralOptionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Mistral.Tests.Configuration;

public class MistralOptionsTests
{
    [Fact]
    public void Defaults_AreProductionSafe()
    {
        // Arrange / Act
        var options = new MistralOptions();

        // Assert
        MistralOptions.SectionName.Should().Be("Mistral");
        options.BaseUrl.Should().Be("https://api.mistral.ai");
        options.DefaultModel.Should().Be("mistral-large-latest");
        options.DefaultEmbeddingModel.Should().Be("mistral-embed");
        options.DefaultTemperature.Should().BeApproximately(0.7f, 0.0001f);
        options.DefaultMaxTokens.Should().Be(4096);
        options.TimeoutSeconds.Should().Be(120);
        options.RetryAttempts.Should().Be(3);
        options.MaxEmbeddingsBatchSize.Should().Be(512);
        options.EnableLogging.Should().BeFalse();
        options.UseStructuredOutputsByDefault.Should().BeFalse();
        options.ApiKey.Should().BeEmpty();
    }

    [Fact]
    public void SectionName_IsStableConstant()
    {
        // The section name must not silently change because consumers bind config to it.
        MistralOptions.SectionName.Should().Be("Mistral");
    }
}
