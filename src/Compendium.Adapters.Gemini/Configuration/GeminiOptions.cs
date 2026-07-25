// -----------------------------------------------------------------------
// <copyright file="GeminiOptions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Gemini.Configuration;

/// <summary>
/// Configuration options for the Gemini AI provider.
/// </summary>
public sealed class GeminiOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Gemini";

    /// <summary>
    /// Gets or sets the Gemini API key. Required.
    /// Authenticated via the <c>?key=</c> query string parameter on every request.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the Gemini API.
    /// Default is <c>https://generativelanguage.googleapis.com</c>.
    /// </summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com";

    /// <summary>
    /// Gets or sets the API version segment under the base URL.
    /// Default is <c>v1beta</c> (where tools, structured outputs, and streaming live today).
    /// </summary>
    public string ApiVersion { get; set; } = "v1beta";

    /// <summary>
    /// Gets or sets the default chat model.
    /// Default is <c>gemini-2.0-flash</c>.
    /// </summary>
    public string DefaultModel { get; set; } = "gemini-2.0-flash";

    /// <summary>
    /// Gets or sets the default embedding model.
    /// Default is <c>text-embedding-004</c>.
    /// </summary>
    public string DefaultEmbeddingModel { get; set; } = "text-embedding-004";

    /// <summary>
    /// Gets or sets the default sampling temperature.
    /// </summary>
    public float DefaultTemperature { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets the default maximum tokens for chat completions
    /// (sent to Gemini as <c>generationConfig.maxOutputTokens</c>).
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
    /// Gets or sets whether structured outputs (<c>responseMimeType: application/json</c>) is enabled
    /// by default for every completion request. Individual calls can still opt in/out via
    /// <see cref="CompletionRequest.AdditionalParameters"/>.
    /// </summary>
    public bool UseStructuredOutputsByDefault { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of inputs to send per <c>batchEmbedContents</c> request.
    /// Gemini caps batched embeddings at 100 inputs per call.
    /// </summary>
    public int MaxEmbeddingsBatchSize { get; set; } = 100;
}
