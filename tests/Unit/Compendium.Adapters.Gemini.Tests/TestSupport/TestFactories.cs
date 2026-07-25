// -----------------------------------------------------------------------
// <copyright file="TestFactories.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Gemini.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Compendium.Adapters.Gemini.Tests.TestSupport;

internal static class TestFactories
{
    public const string DefaultBaseUrl = "https://generativelanguage.googleapis.com";
    public const string DefaultApiKey = "test-api-key";

    public static GeminiOptions DefaultOptions(Action<GeminiOptions>? configure = null)
    {
        var options = new GeminiOptions
        {
            ApiKey = DefaultApiKey,
            BaseUrl = DefaultBaseUrl,
            ApiVersion = "v1beta",
            DefaultModel = "gemini-2.0-flash",
            DefaultEmbeddingModel = "text-embedding-004",
            DefaultMaxTokens = 4096,
            TimeoutSeconds = 120,
            EnableLogging = false,
            MaxEmbeddingsBatchSize = 100
        };
        configure?.Invoke(options);
        return options;
    }

    public static (GeminiHttpClient Client, MockHttpMessageHandler Handler) CreateHttpClient(
        Action<GeminiOptions>? configure = null)
    {
        var handler = new MockHttpMessageHandler();
        var options = DefaultOptions(configure);
        var httpClient = new HttpClient(handler);
        var sut = new GeminiHttpClient(
            httpClient,
            Options.Create(options),
            NullLogger<GeminiHttpClient>.Instance);
        return (sut, handler);
    }

    public static GeminiAIProvider CreateProvider(
        GeminiHttpClient httpClient,
        Action<GeminiOptions>? configure = null)
    {
        var options = DefaultOptions(configure);
        return new GeminiAIProvider(
            httpClient,
            Options.Create(options),
            NullLogger<GeminiAIProvider>.Instance);
    }

    public static CompletionRequest SimpleCompletionRequest(string? model = null) =>
        new()
        {
            Model = model ?? "gemini-2.0-flash",
            Messages = new List<Message> { Message.User("Hello") }
        };

    public static EmbeddingRequest SimpleEmbeddingRequest(int n = 1, string? model = null)
    {
        return new EmbeddingRequest
        {
            Model = model ?? "text-embedding-004",
            Inputs = Enumerable.Range(0, n).Select(i => $"input-{i}").ToList()
        };
    }
}
