// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Mercury.Configuration;
using Compendium.Adapters.Mercury.Http;
using Compendium.Adapters.Mercury.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compendium.Adapters.Mercury.DependencyInjection;

/// <summary>
/// Extension methods for registering the Mercury AI provider with DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IAIProvider"/> backed by Mercury (Inception Labs), binding
    /// configuration from the supplied <paramref name="configuration"/> section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">An <see cref="IConfiguration"/> rooted at the Mercury options
    /// section (typically <c>configuration.GetSection("Mercury")</c>) or any parent
    /// configuration if the section is named <see cref="MercuryOptions.SectionName"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCompendiumMercury(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<MercuryOptions>(configuration);
        return services.AddCompendiumMercuryCore();
    }

    /// <summary>
    /// Registers <see cref="IAIProvider"/> backed by Mercury (Inception Labs) using an inline
    /// configuration callback.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Callback to mutate <see cref="MercuryOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCompendiumMercury(
        this IServiceCollection services,
        Action<MercuryOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        return services.AddCompendiumMercuryCore();
    }

    private static IServiceCollection AddCompendiumMercuryCore(this IServiceCollection services)
    {
        services.AddHttpClient<MercuryHttpClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<MercuryOptions>>().Value;
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddStandardResilienceHandler();

        services.AddSingleton<MercuryAIProvider>();
        services.AddSingleton<IAIProvider>(sp => sp.GetRequiredService<MercuryAIProvider>());

        return services;
    }
}
