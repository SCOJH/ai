// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.LiteLLM.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compendium.Adapters.LiteLLM.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCompendiumLiteLLM_WithConfiguration_RegistersIAIProvider()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            ["LiteLLM:BaseUrl"] = "http://litellm.internal:4000",
            ["LiteLLM:VirtualKey"] = "vk-bound",
            ["LiteLLM:DefaultModel"] = "openai/gpt-4o"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCompendiumLiteLLM(config);
        var provider = services.BuildServiceProvider();

        // Assert
        var aiProvider = provider.GetService<IAIProvider>();
        aiProvider.Should().NotBeNull();
        aiProvider!.ProviderId.Should().Be("litellm");
        var opts = provider.GetRequiredService<IOptions<LiteLLMOptions>>().Value;
        opts.BaseUrl.Should().Be("http://litellm.internal:4000");
        opts.VirtualKey.Should().Be("vk-bound");
        opts.DefaultModel.Should().Be("openai/gpt-4o");
    }

    [Fact]
    public void AddCompendiumLiteLLM_WithActionConfigure_RegistersIAIProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCompendiumLiteLLM(opt =>
        {
            opt.ApiKey = "plain";
            opt.DefaultModel = "anthropic/claude-sonnet-4";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var aiProvider = provider.GetRequiredService<IAIProvider>();
        aiProvider.ProviderId.Should().Be("litellm");
        provider.GetRequiredService<IOptions<LiteLLMOptions>>().Value.DefaultModel.Should().Be("anthropic/claude-sonnet-4");
    }

    [Fact]
    public void AddCompendiumLiteLLM_WithActionConfigure_RegistersHttpClientFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCompendiumLiteLLM(o =>
        {
            o.ApiKey = "plain";
            o.TimeoutSeconds = 42;
        });
        var sp = services.BuildServiceProvider();

        // Assert
        sp.GetService<IHttpClientFactory>().Should().NotBeNull();
        sp.GetRequiredService<IOptions<LiteLLMOptions>>().Value.TimeoutSeconds.Should().Be(42);
    }

    [Fact]
    public void AddCompendiumLiteLLM_ResolvesIAIProviderAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompendiumLiteLLM(o => o.ApiKey = "plain");
        var sp = services.BuildServiceProvider();

        // Act
        var first = sp.GetRequiredService<IAIProvider>();
        var second = sp.GetRequiredService<IAIProvider>();

        // Assert
        first.Should().BeSameAs(second);
    }

    [Fact]
    public void AddCompendiumLiteLLM_NullServices_Throws()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddCompendiumLiteLLM(_ => { });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumLiteLLM_NullConfiguration_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddCompendiumLiteLLM((IConfiguration)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumLiteLLM_NullAction_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddCompendiumLiteLLM((Action<LiteLLMOptions>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
