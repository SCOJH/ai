// -----------------------------------------------------------------------
// <copyright file="AnthropicModelCatalog.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Anthropic.Services;

/// <summary>
/// Static, hand-curated catalog of Claude models known at adapter ship time.
/// Anthropic does not expose <c>/v1/models</c> on the public surface covered by
/// this preview; callers can still target any model id via
/// <see cref="CompletionRequest.Model"/>.
/// </summary>
internal static class AnthropicModelCatalog
{
    /// <summary>Curated list returned from <see cref="IAIProvider.ListModelsAsync"/>.</summary>
    public static readonly IReadOnlyList<AIModel> KnownModels = new AIModel[]
    {
        new()
        {
            Id = "claude-opus-4-5",
            Name = "Claude Opus 4.5",
            Provider = "anthropic",
            ContextWindow = 200_000,
            MaxOutputTokens = 8_192,
            SupportsStreaming = true,
            SupportsEmbeddings = false,
            SupportsVision = true,
            SupportsTools = true,
            PricingInputPerMillion = 15m,
            PricingOutputPerMillion = 75m,
        },
        new()
        {
            Id = "claude-sonnet-4-5",
            Name = "Claude Sonnet 4.5",
            Provider = "anthropic",
            ContextWindow = 200_000,
            MaxOutputTokens = 8_192,
            SupportsStreaming = true,
            SupportsEmbeddings = false,
            SupportsVision = true,
            SupportsTools = true,
            PricingInputPerMillion = 3m,
            PricingOutputPerMillion = 15m,
        },
        new()
        {
            Id = "claude-3-7-sonnet-latest",
            Name = "Claude 3.7 Sonnet",
            Provider = "anthropic",
            ContextWindow = 200_000,
            MaxOutputTokens = 8_192,
            SupportsStreaming = true,
            SupportsEmbeddings = false,
            SupportsVision = true,
            SupportsTools = true,
            PricingInputPerMillion = 3m,
            PricingOutputPerMillion = 15m,
        },
        new()
        {
            Id = "claude-3-5-haiku-latest",
            Name = "Claude 3.5 Haiku",
            Provider = "anthropic",
            ContextWindow = 200_000,
            MaxOutputTokens = 8_192,
            SupportsStreaming = true,
            SupportsEmbeddings = false,
            SupportsVision = false,
            SupportsTools = true,
            PricingInputPerMillion = 0.80m,
            PricingOutputPerMillion = 4m,
        },
    };
}
