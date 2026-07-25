// -----------------------------------------------------------------------
// <copyright file="MistralOptions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Mistral.Configuration;

/// <summary>
/// Configuration options for the Mistral AI provider ("la Plateforme").
/// </summary>
/// <remarks>
/// Mistral hosts its inference in the EU (France). This adapter is the preferred
/// <see cref="IAIProvider"/> implementation for tenants that require GDPR-friendly
/// data residency — payloads never leave the EU.
/// </remarks>
public sealed class MistralOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Mistral";

    /// <summary>
    /// Gets or sets the Mistral API key. Required.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the Mistral API.
    /// Default is "https://api.mistral.ai".
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.mistral.ai";

    /// <summary>
    /// Gets or sets the default chat model.
    /// Default is "mistral-large-latest".
    /// </summary>
    public string DefaultModel { get; set; } = "mistral-large-latest";

    /// <summary>
    /// Gets or sets the default embedding model.
    /// Default is "mistral-embed".
    /// </summary>
    public string DefaultEmbeddingModel { get; set; } = "mistral-embed";

    /// <summary>
    /// Gets or sets the default sampling temperature.
    /// </summary>
    public float DefaultTemperature { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets the default maximum tokens for chat completions.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 4096;

    /// <summary>
    /// Gets or sets the HTTP timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the number of retry attempts for transient failures.
    /// Applied via Microsoft.Extensions.Http.Resilience's standard pipeline.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to enable verbose request/response logging at debug level.
    /// </summary>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Gets or sets whether structured outputs (JSON-object response_format) is enabled by default for
    /// every completion request. Individual calls can still opt in/out via <see cref="CompletionRequest.AdditionalParameters"/>.
    /// </summary>
    public bool UseStructuredOutputsByDefault { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of inputs to send per embeddings request.
    /// Mistral's documented practical limit is 512; we batch in chunks no larger than this.
    /// </summary>
    public int MaxEmbeddingsBatchSize { get; set; } = 512;
}
