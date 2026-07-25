// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Mercury.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Compendium.Adapters.Mercury.Tests.DependencyInjection;

/// <summary>
/// Unit tests for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    private static IServiceCollection BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        return services;
    }

    [Fact]
    public void AddCompendiumMercury_WithConfigureAction_RegistersOptionsAndProvider()
    {
        // Arrange
        var services = BuildServices();

        // Act
        services.AddCompendiumMercury(o => o.ApiKey = "sk-mercury-abc");
        var sp = services.BuildServiceProvider();

        // Assert
        sp.GetService<IOptions<MercuryOptions>>().Should().NotBeNull();
        sp.GetService<IOptions<MercuryOptions>>()!.Value.ApiKey.Should().Be("sk-mercury-abc");
        sp.GetService<IAIProvider>().Should().NotBeNull();
        sp.GetService<IAIProvider>()!.ProviderId.Should().Be("mercury");
    }

    [Fact]
    public void AddCompendiumMercury_WithConfigureAction_RegistersHttpClientFactory()
    {
        // Arrange
        var services = BuildServices();

        // Act
        services.AddCompendiumMercury(o =>
        {
            o.ApiKey = "sk";
            o.TimeoutSeconds = 42;
        });
        var sp = services.BuildServiceProvider();

        // Assert
        var factory = sp.GetService<IHttpClientFactory>();
        factory.Should().NotBeNull();
        sp.GetRequiredService<IOptions<MercuryOptions>>().Value.TimeoutSeconds.Should().Be(42);
    }

    [Fact]
    public void AddCompendiumMercury_WithConfigureAction_ResolvesIAIProviderAsSingleton()
    {
        // Arrange
        var services = BuildServices();
        services.AddCompendiumMercury(o => o.ApiKey = "sk");
        var sp = services.BuildServiceProvider();

        // Act
        var first = sp.GetRequiredService<IAIProvider>();
        var second = sp.GetRequiredService<IAIProvider>();

        // Assert
        first.Should().BeSameAs(second);
    }

    [Fact]
    public void AddCompendiumMercury_WithConfiguration_BindsMercurySection()
    {
        // Arrange
        var services = BuildServices();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKey"] = "sk-bound",
                ["DefaultModel"] = "mercury-coder",
                ["DefaultMaxTokens"] = "8192",
                ["TimeoutSeconds"] = "30",
                ["EnableLogging"] = "true",
            })
            .Build();

        // Act
        services.AddCompendiumMercury(config);
        var sp = services.BuildServiceProvider();

        // Assert
        var opts = sp.GetRequiredService<IOptions<MercuryOptions>>().Value;
        opts.ApiKey.Should().Be("sk-bound");
        opts.DefaultModel.Should().Be("mercury-coder");
        opts.DefaultMaxTokens.Should().Be(8192);
        opts.TimeoutSeconds.Should().Be(30);
        opts.EnableLogging.Should().BeTrue();
        sp.GetService<IAIProvider>().Should().NotBeNull();
    }

    [Fact]
    public void AddCompendiumMercury_WithConfigureAction_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = BuildServices();

        // Act
        var returned = services.AddCompendiumMercury(o => o.ApiKey = "sk");

        // Assert
        returned.Should().BeSameAs(services);
    }

    [Fact]
    public void AddCompendiumMercury_WithConfiguration_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = BuildServices();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiKey"] = "sk" })
            .Build();

        // Act
        var returned = services.AddCompendiumMercury(config);

        // Assert
        returned.Should().BeSameAs(services);
    }

    [Fact]
    public void AddCompendiumMercury_NullServices_WithConfigureAction_Throws()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddCompendiumMercury(_ => { });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumMercury_NullConfigureAction_Throws()
    {
        // Arrange
        var services = BuildServices();
        Action<MercuryOptions>? configure = null;

        // Act
        var act = () => services.AddCompendiumMercury(configure!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumMercury_NullServices_WithConfiguration_Throws()
    {
        // Arrange
        IServiceCollection? services = null;
        var config = new ConfigurationBuilder().Build();

        // Act
        var act = () => services!.AddCompendiumMercury(config);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCompendiumMercury_NullConfiguration_Throws()
    {
        // Arrange
        var services = BuildServices();
        IConfiguration? config = null;

        // Act
        var act = () => services.AddCompendiumMercury(config!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
