// -----------------------------------------------------------------------
// <copyright file="TestFactories.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.OpenAI.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Compendium.Adapters.OpenAI.Tests.TestSupport;

internal static class TestFactories
{
    public const string DefaultBaseUrl = "https://api.openai.com/v1";
    public const string DefaultApiKey = "sk-test-key";

    public static OpenAIOptions DefaultOptions(Action<OpenAIOptions>? configure = null)
    {
        var options = new OpenAIOptions
        {
            ApiKey = DefaultApiKey,
            BaseUrl = DefaultBaseUrl,
            DefaultModel = "gpt-4o-mini",
            DefaultEmbeddingModel = "text-embedding-3-small",
            DefaultMaxTokens = 4096,
            TimeoutSeconds = 120,
            EnableLogging = false
        };
        configure?.Invoke(options);
        return options;
    }

    public static (OpenAIHttpClient Client, MockHttpMessageHandler Handler) CreateHttpClient(
        Action<OpenAIOptions>? configure = null)
    {
        var handler = new MockHttpMessageHandler();
        var options = DefaultOptions(configure);
        var httpClient = new HttpClient(handler);
        var sut = new OpenAIHttpClient(
            httpClient,
            Options.Create(options),
            NullLogger<OpenAIHttpClient>.Instance);
        return (sut, handler);
    }

    public static OpenAIAIProvider CreateProvider(
        OpenAIHttpClient httpClient,
        Action<OpenAIOptions>? configure = null)
    {
        var options = DefaultOptions(configure);
        return new OpenAIAIProvider(
            httpClient,
            Options.Create(options),
            NullLogger<OpenAIAIProvider>.Instance);
    }

    public static CompletionRequest SimpleCompletionRequest(string? model = null) =>
        new()
        {
            Model = model ?? "gpt-4o-mini",
            Messages = new List<Message> { Message.User("Hello") }
        };

    public static EmbeddingRequest SimpleEmbeddingRequest(int n = 1, string? model = null)
    {
        return new EmbeddingRequest
        {
            Model = model ?? "text-embedding-3-small",
            Inputs = Enumerable.Range(0, n).Select(i => $"input-{i}").ToList()
        };
    }
}
