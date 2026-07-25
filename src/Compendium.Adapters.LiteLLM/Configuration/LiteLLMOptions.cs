// -----------------------------------------------------------------------
// <copyright file="LiteLLMOptions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.LiteLLM.Configuration;

/// <summary>
/// Configuration options for the LiteLLM proxy AI provider.
/// </summary>
/// <remarks>
/// LiteLLM is a self-hostable gateway that exposes 100+ LLM providers behind an OpenAI-compatible
/// REST API. The defaults target a local LiteLLM proxy at <c>http://localhost:4000</c>. Point
/// <see cref="BaseUrl"/> at your cloud / cluster deployment for production usage.
/// </remarks>
public sealed class LiteLLMOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "LiteLLM";

    /// <summary>
    /// Gets or sets the API key used as a bearer token for the LiteLLM proxy.
    /// Optional — local self-hosted proxies often run without auth. When set, it is sent as
    /// <c>Authorization: Bearer &lt;ApiKey&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="VirtualKey"/> instead when calling a LiteLLM proxy configured with
    /// virtual-key auth — they live in the same header and only one is forwarded (virtual key wins).
    /// </remarks>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the LiteLLM virtual key (usually prefixed with <c>sk-litellm-</c>). When set,
    /// it overrides <see cref="ApiKey"/> for the outbound <c>Authorization: Bearer</c> header.
    /// Virtual keys carry per-team budget, rate-limit, and model-allow-list policy on the proxy.
    /// </summary>
    public string? VirtualKey { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the LiteLLM proxy.
    /// Default is <c>http://localhost:4000</c> (the standard self-hosted endpoint).
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:4000";

    /// <summary>
    /// Gets or sets the default model id to use when not specified on the request.
    /// LiteLLM uses provider-prefixed identifiers like <c>openai/gpt-4o</c>,
    /// <c>anthropic/claude-sonnet-4</c>, or <c>bedrock/anthropic.claude-3-haiku</c>.
    /// </summary>
    public string DefaultModel { get; set; } = "openai/gpt-4o-mini";

    /// <summary>
    /// Gets or sets the default embedding model id (also provider-prefixed).
    /// </summary>
    public string DefaultEmbeddingModel { get; set; } = "openai/text-embedding-3-small";

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
    /// Gets or sets whether <c>response_format = {"type":"json_object"}</c> is applied by default
    /// for every completion. Individual calls can still opt in/out via
    /// <see cref="CompletionRequest.AdditionalParameters"/>. Whether the upstream provider honours
    /// this depends on its own capability set — LiteLLM passes it through unchanged.
    /// </summary>
    public bool UseStructuredOutputsByDefault { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of inputs to send per embeddings request.
    /// </summary>
    public int MaxEmbeddingsBatchSize { get; set; } = 2048;

    /// <summary>
    /// Gets or sets additional headers to forward on every outbound request. Useful for
    /// LiteLLM-specific routing primitives such as <c>x-litellm-cache-key</c>,
    /// <c>x-litellm-tags</c>, or any custom header your proxy chain needs.
    /// </summary>
    public Dictionary<string, string> PassThroughHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Validates the options.
    /// </summary>
    /// <returns><c>true</c> when usable against a configured proxy — i.e. <see cref="BaseUrl"/>
    /// is set and parseable as an absolute URI. Auth is optional because many self-hosted
    /// LiteLLM proxies run with no auth at all.</returns>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(BaseUrl)
        && Uri.TryCreate(BaseUrl, UriKind.Absolute, out _);

    /// <summary>
    /// Gets the effective bearer credential used on outbound requests: <see cref="VirtualKey"/> wins
    /// when set, otherwise <see cref="ApiKey"/>. Returns <c>null</c> when neither is configured.
    /// </summary>
    internal string? GetEffectiveAuthorizationToken()
    {
        if (!string.IsNullOrWhiteSpace(VirtualKey))
        {
            return VirtualKey;
        }
        return string.IsNullOrWhiteSpace(ApiKey) ? null : ApiKey;
    }
}
