// -----------------------------------------------------------------------
// <copyright file="TestFactories.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.LiteLLM.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Compendium.Adapters.LiteLLM.Tests.TestSupport;

internal static class TestFactories
{
    public const string DefaultBaseUrl = "http://localhost:4000";
    public const string DefaultApiKey = "test-proxy-key";

    public static LiteLLMOptions DefaultOptions(Action<LiteLLMOptions>? configure = null)
    {
        var options = new LiteLLMOptions
        {
            ApiKey = DefaultApiKey,
            BaseUrl = DefaultBaseUrl,
            DefaultModel = "openai/gpt-4o-mini",
            DefaultEmbeddingModel = "openai/text-embedding-3-small",
            DefaultMaxTokens = 4096,
            TimeoutSeconds = 120,
            EnableLogging = false
        };
        configure?.Invoke(options);
        return options;
    }

    public static (LiteLLMHttpClient Client, MockHttpMessageHandler Handler) CreateHttpClient(
        Action<LiteLLMOptions>? configure = null)
    {
        var handler = new MockHttpMessageHandler();
        var options = DefaultOptions(configure);
        var httpClient = new HttpClient(handler);
        var sut = new LiteLLMHttpClient(
            httpClient,
            Options.Create(options),
            NullLogger<LiteLLMHttpClient>.Instance);
        return (sut, handler);
    }

    public static LiteLLMAIProvider CreateProvider(
        LiteLLMHttpClient httpClient,
        Action<LiteLLMOptions>? configure = null)
    {
        var options = DefaultOptions(configure);
        return new LiteLLMAIProvider(
            httpClient,
            Options.Create(options),
            NullLogger<LiteLLMAIProvider>.Instance);
    }

    public static CompletionRequest SimpleCompletionRequest(string? model = null) =>
        new()
        {
            Model = model ?? "openai/gpt-4o-mini",
            Messages = new List<Message> { Message.User("Hello") }
        };

    public static EmbeddingRequest SimpleEmbeddingRequest(int n = 1, string? model = null)
    {
        return new EmbeddingRequest
        {
            Model = model ?? "openai/text-embedding-3-small",
            Inputs = Enumerable.Range(0, n).Select(i => $"input-{i}").ToList()
        };
    }

    /// <summary>Recording logger used to verify log emission.</summary>
    public sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception), exception));
        private sealed class NullScope : IDisposable { public static readonly NullScope Instance = new(); public void Dispose() { } }
    }
}
