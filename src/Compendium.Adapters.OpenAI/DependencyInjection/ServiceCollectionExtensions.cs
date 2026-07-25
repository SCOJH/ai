// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.OpenAI.Configuration;
using Compendium.Adapters.OpenAI.Http;
using Compendium.Adapters.OpenAI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compendium.Adapters.OpenAI.DependencyInjection;

/// <summary>
/// DI extensions for the OpenAI Compendium adapter.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers OpenAI as the <see cref="IAIProvider"/> with options bound from
    /// <paramref name="configuration"/> at section <see cref="OpenAIOptions.SectionName"/>.
    /// </summary>
    public static IServiceCollection AddCompendiumOpenAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.SectionName));
        return services.AddCompendiumOpenAICore();
    }

    /// <summary>
    /// Registers OpenAI as the <see cref="IAIProvider"/> with options configured inline.
    /// </summary>
    public static IServiceCollection AddCompendiumOpenAI(
        this IServiceCollection services,
        Action<OpenAIOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        return services.AddCompendiumOpenAICore();
    }

    private static IServiceCollection AddCompendiumOpenAICore(this IServiceCollection services)
    {
        services.AddHttpClient<OpenAIHttpClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddStandardResilienceHandler();

        services.AddSingleton<OpenAIAIProvider>();
        services.AddSingleton<IAIProvider>(sp => sp.GetRequiredService<OpenAIAIProvider>());

        return services;
    }
}
