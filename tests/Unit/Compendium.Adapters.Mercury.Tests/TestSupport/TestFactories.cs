// -----------------------------------------------------------------------
// <copyright file="TestFactories.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Mercury.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Compendium.Adapters.Mercury.Tests.TestSupport;

/// <summary>
/// Helpers for constructing internal SUTs (HttpClient + Options) in unit tests.
/// </summary>
internal static class TestFactories
{
    public const string DefaultBaseUrl = "https://api.inceptionlabs.ai/v1";
    public const string DefaultApiKey = "sk-mercury-test-key";

    public static MercuryOptions DefaultOptions(Action<MercuryOptions>? configure = null)
    {
        var options = new MercuryOptions
        {
            ApiKey = DefaultApiKey,
            BaseUrl = DefaultBaseUrl,
            DefaultModel = "mercury",
            DefaultMaxTokens = 4096,
            TimeoutSeconds = 120,
            EnableLogging = false,
        };
        configure?.Invoke(options);
        return options;
    }

    public static (MercuryHttpClient Client, MockHttpMessageHandler Handler) CreateHttpClient(
        Action<MercuryOptions>? configure = null)
    {
        var handler = new MockHttpMessageHandler();
        var options = DefaultOptions(configure);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(options.BaseUrl),
        };
        var sut = new MercuryHttpClient(
            httpClient,
            Options.Create(options),
            NullLogger<MercuryHttpClient>.Instance);
        return (sut, handler);
    }

    public static MercuryAIProvider CreateProvider(
        MercuryHttpClient httpClient,
        Action<MercuryOptions>? configure = null)
    {
        var options = DefaultOptions(configure);
        return new MercuryAIProvider(
            httpClient,
            Options.Create(options),
            NullLogger<MercuryAIProvider>.Instance);
    }

    public static CompletionRequest SimpleCompletionRequest(string? model = null)
    {
        return new CompletionRequest
        {
            Model = model ?? "mercury",
            Messages = new List<Message> { Message.User("Hello") },
        };
    }

    /// <summary>
    /// Recording logger used to verify that log methods were invoked.
    /// </summary>
    public sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception), exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose() { }
        }
    }
}
