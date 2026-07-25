// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Mistral.Configuration;
using Compendium.Adapters.Mistral.Http;
using Compendium.Adapters.Mistral.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compendium.Adapters.Mistral.DependencyInjection;

/// <summary>
/// DI extensions for the Mistral Compendium adapter.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Mistral as the <see cref="IAIProvider"/> with options bound from
    /// <paramref name="configuration"/> at section <see cref="MistralOptions.SectionName"/>.
    /// </summary>
    public static IServiceCollection AddCompendiumMistral(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<MistralOptions>(configuration.GetSection(MistralOptions.SectionName));
        return services.AddCompendiumMistralCore();
    }

    /// <summary>
    /// Registers Mistral as the <see cref="IAIProvider"/> with options configured inline.
    /// </summary>
    public static IServiceCollection AddCompendiumMistral(
        this IServiceCollection services,
        Action<MistralOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        return services.AddCompendiumMistralCore();
    }

    private static IServiceCollection AddCompendiumMistralCore(this IServiceCollection services)
    {
        services.AddHttpClient<MistralHttpClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MistralOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddStandardResilienceHandler();

        services.AddSingleton<MistralAIProvider>();
        services.AddSingleton<IAIProvider>(sp => sp.GetRequiredService<MistralAIProvider>());

        return services;
    }
}
