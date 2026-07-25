// -----------------------------------------------------------------------
// <copyright file="OpenAIOptionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.OpenAI.Tests.Configuration;

public class OpenAIOptionsTests
{
    [Fact]
    public void Defaults_AreProductionSafe()
    {
        // Arrange / Act
        var options = new OpenAIOptions();

        // Assert
        OpenAIOptions.SectionName.Should().Be("OpenAI");
        options.BaseUrl.Should().Be("https://api.openai.com/v1");
        options.DefaultModel.Should().Be("gpt-4o-mini");
        options.DefaultEmbeddingModel.Should().Be("text-embedding-3-small");
        options.DefaultTemperature.Should().BeApproximately(0.7f, 0.0001f);
        options.DefaultMaxTokens.Should().Be(4096);
        options.TimeoutSeconds.Should().Be(120);
        options.RetryAttempts.Should().Be(3);
        options.MaxEmbeddingsBatchSize.Should().Be(2048);
        options.EnableLogging.Should().BeFalse();
        options.UseStructuredOutputsByDefault.Should().BeFalse();
        options.ApiKey.Should().BeEmpty();
        options.Organization.Should().BeNull();
        options.Project.Should().BeNull();
    }

    [Fact]
    public void SectionName_IsStableConstant()
    {
        // The section name must not silently change because consumers bind config to it.
        OpenAIOptions.SectionName.Should().Be("OpenAI");
    }
}
