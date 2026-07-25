// -----------------------------------------------------------------------
// <copyright file="MercuryOptions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Mercury.Configuration;

/// <summary>
/// Configuration options for the Mercury (Inception Labs) AI provider.
/// </summary>
/// <remarks>
/// Mercury is a diffusion-based LLM. The HTTP surface is OpenAI-compatible — point
/// <see cref="BaseUrl"/> at Inception Labs' v1 endpoint and authenticate with a
/// bearer API key. Differentiating value: very high throughput (5-10x autoregressive
/// peers per the vendor's docs), well-suited to interactive autocomplete, voice
/// agents, and tight chat loops.
/// </remarks>
public sealed class MercuryOptions
{
    /// <summary>
    /// The configuration section name (<c>"Mercury"</c>).
    /// </summary>
    public const string SectionName = "Mercury";

    /// <summary>
    /// Gets or sets the Mercury API key. Required.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the Mercury API.
    /// Default is <c>"https://api.inceptionlabs.ai/v1"</c>.
    /// </summary>
    /// <remarks>
    /// Mercury's API surface is OpenAI-compatible. If Inception Labs moves endpoints,
    /// override this value rather than waiting for a new adapter release.
    /// </remarks>
    public string BaseUrl { get; set; } = "https://api.inceptionlabs.ai/v1";

    /// <summary>
    /// Gets or sets the default model to use when a request does not specify one.
    /// Default is <c>"mercury"</c>.
    /// </summary>
    /// <remarks>
    /// Known catalogue (May 2026): <c>mercury</c>, <c>mercury-2</c>, <c>mercury-coder</c>,
    /// <c>mercury-edit</c>, <c>mercury-edit-2</c>. Tool calling and JSON mode are
    /// advertised on <c>mercury</c>, <c>mercury-2</c>, and <c>mercury-coder</c>.
    /// </remarks>
    public string DefaultModel { get; set; } = "mercury";

    /// <summary>
    /// Gets or sets the default temperature applied when not provided in the request.
    /// </summary>
    public float DefaultTemperature { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets the default maximum tokens applied when not provided in the request.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 4096;

    /// <summary>
    /// Gets or sets the HTTP timeout in seconds. Default is 120.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the number of retry attempts for transient failures.
    /// Forwarded to <c>Microsoft.Extensions.Http.Resilience</c>'s standard handler.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to log request and response bodies at <see cref="LogLevel.Debug"/>.
    /// Off by default. Do not enable in production — bodies may contain prompts / PII.
    /// </summary>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Validates that the minimum configuration (an API key) is present.
    /// </summary>
    /// <returns><c>true</c> when an API key is set; otherwise <c>false</c>.</returns>
    public bool IsValid() => !string.IsNullOrWhiteSpace(ApiKey);
}
