// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.OpenAI.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compendium.Adapters.OpenAI.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCompendiumOpenAI_WithConfiguration_RegistersIAIProvider()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            ["OpenAI:ApiKey"] = "sk-test",
            ["OpenAI:DefaultModel"] = "gpt-4o-mini"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCompendiumOpenAI(config);
        var provider = services.BuildServiceProvider();

        // Assert
        var aiProvider = provider.GetService<IAIProvider>();
        aiProvider.Should().NotBeNull();
        aiProvider!.ProviderId.Should().Be("openai");
        provider.GetRequiredService<IOptions<OpenAIOptions>>().Value.ApiKey.Should().Be("sk-test");
    }

    [Fact]
    public void AddCompendiumOpenAI_WithActionConfigure_RegistersIAIProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCompendiumOpenAI(opt =>
        {
            opt.ApiKey = "sk-action";
            opt.DefaultModel = "gpt-4o";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var aiProvider = provider.GetRequiredService<IAIProvider>();
        aiProvider.ProviderId.Should().Be("openai");
        provider.GetRequiredService<IOptions<OpenAIOptions>>().Value.DefaultModel.Should().Be("gpt-4o");
    }

    [Fact]
    public void AddCompendiumOpenAI_NullServices_Throws()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddCompendiumOpenAI(_ => { });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumOpenAI_NullConfiguration_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddCompendiumOpenAI((IConfiguration)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumOpenAI_NullAction_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddCompendiumOpenAI((Action<OpenAIOptions>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
