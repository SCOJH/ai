// -----------------------------------------------------------------------
// <copyright file="OpenAIOptions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.OpenAI.Configuration;

/// <summary>
/// Configuration options for the OpenAI AI provider.
/// </summary>
public sealed class OpenAIOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "OpenAI";

    /// <summary>
    /// Gets or sets the OpenAI API key. Required.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the OpenAI API.
    /// Default is "https://api.openai.com/v1".
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>
    /// Gets or sets the default chat model.
    /// Default is "gpt-4o-mini".
    /// </summary>
    public string DefaultModel { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Gets or sets the default embedding model.
    /// Default is "text-embedding-3-small".
    /// </summary>
    public string DefaultEmbeddingModel { get; set; } = "text-embedding-3-small";

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
    /// Gets or sets the optional OpenAI organization id (sent as the <c>OpenAI-Organization</c> header).
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Gets or sets the optional OpenAI project id (sent as the <c>OpenAI-Project</c> header).
    /// </summary>
    public string? Project { get; set; }

    /// <summary>
    /// Gets or sets whether to enable verbose request/response logging at debug level.
    /// </summary>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Gets or sets whether structured outputs (JSON-schema response_format) is enabled by default for
    /// every completion request. Individual calls can still opt in/out via <see cref="CompletionRequest.AdditionalParameters"/>.
    /// </summary>
    public bool UseStructuredOutputsByDefault { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of inputs to send per embeddings request.
    /// OpenAI's documented hard limit is 2048; we batch in chunks no larger than this.
    /// </summary>
    public int MaxEmbeddingsBatchSize { get; set; } = 2048;
}
