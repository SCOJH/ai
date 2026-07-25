// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Mistral.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compendium.Adapters.Mistral.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCompendiumMistral_WithConfiguration_RegistersIAIProvider()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            ["Mistral:ApiKey"] = "test-key",
            ["Mistral:DefaultModel"] = "mistral-small-latest"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCompendiumMistral(config);
        var provider = services.BuildServiceProvider();

        // Assert
        var aiProvider = provider.GetService<IAIProvider>();
        aiProvider.Should().NotBeNull();
        aiProvider!.ProviderId.Should().Be("mistral");
        provider.GetRequiredService<IOptions<MistralOptions>>().Value.ApiKey.Should().Be("test-key");
        provider.GetRequiredService<IOptions<MistralOptions>>().Value.DefaultModel.Should().Be("mistral-small-latest");
    }

    [Fact]
    public void AddCompendiumMistral_WithActionConfigure_RegistersIAIProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCompendiumMistral(opt =>
        {
            opt.ApiKey = "action-key";
            opt.DefaultModel = "codestral-latest";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var aiProvider = provider.GetRequiredService<IAIProvider>();
        aiProvider.ProviderId.Should().Be("mistral");
        provider.GetRequiredService<IOptions<MistralOptions>>().Value.DefaultModel.Should().Be("codestral-latest");
    }

    [Fact]
    public void AddCompendiumMistral_NullServices_Throws()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddCompendiumMistral(_ => { });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumMistral_NullConfiguration_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddCompendiumMistral((IConfiguration)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumMistral_NullAction_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddCompendiumMistral((Action<MistralOptions>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
