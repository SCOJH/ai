// -----------------------------------------------------------------------
// <copyright file="TestFactories.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Mistral.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Compendium.Adapters.Mistral.Tests.TestSupport;

internal static class TestFactories
{
    // The HttpClient gets a "/v1/" suffix added by MistralHttpClient when the configured
    // BaseUrl doesn't already end in /v1. We pin the bare host here so MockHttp's globbed
    // patterns ("*/chat/completions") match regardless.
    public const string DefaultBaseUrl = "https://api.mistral.ai";
    public const string DefaultApiKey = "test-mistral-key";

    public static MistralOptions DefaultOptions(Action<MistralOptions>? configure = null)
    {
        var options = new MistralOptions
        {
            ApiKey = DefaultApiKey,
            BaseUrl = DefaultBaseUrl,
            DefaultModel = "mistral-large-latest",
            DefaultEmbeddingModel = "mistral-embed",
            DefaultMaxTokens = 4096,
            TimeoutSeconds = 120,
            EnableLogging = false
        };
        configure?.Invoke(options);
        return options;
    }

    public static (MistralHttpClient Client, MockHttpMessageHandler Handler) CreateHttpClient(
        Action<MistralOptions>? configure = null)
    {
        var handler = new MockHttpMessageHandler();
        var options = DefaultOptions(configure);
        var httpClient = new HttpClient(handler);
        var sut = new MistralHttpClient(
            httpClient,
            Options.Create(options),
            NullLogger<MistralHttpClient>.Instance);
        return (sut, handler);
    }

    public static MistralAIProvider CreateProvider(
        MistralHttpClient httpClient,
        Action<MistralOptions>? configure = null)
    {
        var options = DefaultOptions(configure);
        return new MistralAIProvider(
            httpClient,
            Options.Create(options),
            NullLogger<MistralAIProvider>.Instance);
    }

    public static CompletionRequest SimpleCompletionRequest(string? model = null) =>
        new()
        {
            Model = model ?? "mistral-large-latest",
            Messages = new List<Message> { Message.User("Hello") }
        };

    public static EmbeddingRequest SimpleEmbeddingRequest(int n = 1, string? model = null)
    {
        return new EmbeddingRequest
        {
            Model = model ?? "mistral-embed",
            Inputs = Enumerable.Range(0, n).Select(i => $"input-{i}").ToList()
        };
    }
}
