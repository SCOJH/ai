// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Gemini.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compendium.Adapters.Gemini.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCompendiumGemini_WithConfiguration_RegistersProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Gemini:ApiKey"] = "k1",
                ["Gemini:DefaultModel"] = "gemini-2.0-flash"
            })
            .Build();

        // Act
        var returned = services.AddCompendiumGemini(configuration);
        using var sp = returned.BuildServiceProvider();

        // Assert
        returned.Should().BeSameAs(services);
        sp.GetRequiredService<IAIProvider>().Should().BeOfType<GeminiAIProvider>();
        var options = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;
        options.ApiKey.Should().Be("k1");
        options.DefaultModel.Should().Be("gemini-2.0-flash");
    }

    [Fact]
    public void AddCompendiumGemini_WithCallback_RegistersProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddCompendiumGemini(o =>
        {
            o.ApiKey = "k1";
            o.DefaultModel = "gemini-1.5-pro";
        });
        using var sp = services.BuildServiceProvider();

        // Assert
        sp.GetRequiredService<IAIProvider>().Should().NotBeNull();
        sp.GetRequiredService<IOptions<GeminiOptions>>().Value.DefaultModel.Should().Be("gemini-1.5-pro");
    }

    [Fact]
    public void AddCompendiumGemini_RegistersSameInstanceForBothInterfaceAndConcrete()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompendiumGemini(o => o.ApiKey = "k");

        // Act
        using var sp = services.BuildServiceProvider();
        var concrete = sp.GetRequiredService<GeminiAIProvider>();
        var iface = sp.GetRequiredService<IAIProvider>();

        // Assert — singleton, identity should be preserved.
        iface.Should().BeSameAs(concrete);
    }

    [Fact]
    public void AddCompendiumGemini_NullServices_WithConfig_Throws()
    {
        // Arrange
        IServiceCollection? services = null;
        var config = new ConfigurationBuilder().Build();

        // Act
        var act = () => services!.AddCompendiumGemini(config);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumGemini_NullConfiguration_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddCompendiumGemini((IConfiguration)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumGemini_NullServices_WithCallback_Throws()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddCompendiumGemini(_ => { });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumGemini_NullCallback_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddCompendiumGemini((Action<GeminiOptions>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
