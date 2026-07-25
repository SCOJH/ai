// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Gemini.Configuration;
using Compendium.Adapters.Gemini.Http;
using Compendium.Adapters.Gemini.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Compendium.Adapters.Gemini.DependencyInjection;

/// <summary>
/// DI extensions for the Gemini Compendium adapter.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Gemini as the <see cref="IAIProvider"/> with options bound from
    /// <paramref name="configuration"/> at section <see cref="GeminiOptions.SectionName"/>.
    /// </summary>
    public static IServiceCollection AddCompendiumGemini(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        return services.AddCompendiumGeminiCore();
    }

    /// <summary>
    /// Registers Gemini as the <see cref="IAIProvider"/> with options configured inline.
    /// </summary>
    public static IServiceCollection AddCompendiumGemini(
        this IServiceCollection services,
        Action<GeminiOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        return services.AddCompendiumGeminiCore();
    }

    private static IServiceCollection AddCompendiumGeminiCore(this IServiceCollection services)
    {
        services.AddHttpClient<GeminiHttpClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddStandardResilienceHandler();

        services.AddSingleton<GeminiAIProvider>();
        services.AddSingleton<IAIProvider>(sp => sp.GetRequiredService<GeminiAIProvider>());

        return services;
    }
}
