// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.LiteLLM.Configuration;
using Compendium.Adapters.LiteLLM.Http;
using Compendium.Adapters.LiteLLM.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compendium.Adapters.LiteLLM.DependencyInjection;

/// <summary>
/// DI extensions for the LiteLLM Compendium adapter.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers LiteLLM as the <see cref="IAIProvider"/> with options bound from
    /// <paramref name="configuration"/> at section <see cref="LiteLLMOptions.SectionName"/>.
    /// </summary>
    public static IServiceCollection AddCompendiumLiteLLM(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<LiteLLMOptions>(configuration.GetSection(LiteLLMOptions.SectionName));
        return services.AddCompendiumLiteLLMCore();
    }

    /// <summary>
    /// Registers LiteLLM as the <see cref="IAIProvider"/> with options configured inline.
    /// </summary>
    public static IServiceCollection AddCompendiumLiteLLM(
        this IServiceCollection services,
        Action<LiteLLMOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        return services.AddCompendiumLiteLLMCore();
    }

    private static IServiceCollection AddCompendiumLiteLLMCore(this IServiceCollection services)
    {
        services.AddHttpClient<LiteLLMHttpClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<LiteLLMOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddStandardResilienceHandler();

        services.AddSingleton<LiteLLMAIProvider>();
        services.AddSingleton<IAIProvider>(sp => sp.GetRequiredService<LiteLLMAIProvider>());

        return services;
    }
}
