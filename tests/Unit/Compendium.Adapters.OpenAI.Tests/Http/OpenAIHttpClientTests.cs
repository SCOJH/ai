// -----------------------------------------------------------------------
// <copyright file="OpenAIHttpClientTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.OpenAI.Http;
using Compendium.Adapters.OpenAI.Http.Models;
using Compendium.Adapters.OpenAI.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;

namespace Compendium.Adapters.OpenAI.Tests.Http;

public class OpenAIHttpClientTests
{
    [Fact]
    public void Ctor_SetsBearerTokenAndBaseAddress()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o =>
        {
            o.ApiKey = "sk-test-bearer";
            o.Organization = "org-1";
            o.Project = "proj-1";
        });

        // Act
        var sut = new OpenAIHttpClient(inner, Options.Create(options), NullLogger<OpenAIHttpClient>.Instance);

        // Assert
        sut.Should().NotBeNull();
        inner.BaseAddress!.ToString().Should().StartWith("https://api.openai.com/v1");
        inner.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        inner.DefaultRequestHeaders.Authorization!.Parameter.Should().Be("sk-test-bearer");
        inner.DefaultRequestHeaders.GetValues("OpenAI-Organization").Should().Contain("org-1");
        inner.DefaultRequestHeaders.GetValues("OpenAI-Project").Should().Contain("proj-1");
    }

    [Fact]
    public void Ctor_OmitsOptionalHeaders_WhenNotConfigured()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions();

        // Act
        _ = new OpenAIHttpClient(inner, Options.Create(options), NullLogger<OpenAIHttpClient>.Instance);

        // Assert
        inner.DefaultRequestHeaders.Contains("OpenAI-Organization").Should().BeFalse();
        inner.DefaultRequestHeaders.Contains("OpenAI-Project").Should().BeFalse();
    }

    [Fact]
    public void Ctor_DoesNotOverridePreSetBaseAddress()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler) { BaseAddress = new Uri("https://proxy.test/v1/") };
        var options = TestFactories.DefaultOptions();

        // Act
        _ = new OpenAIHttpClient(inner, Options.Create(options), NullLogger<OpenAIHttpClient>.Instance);

        // Assert
        inner.BaseAddress!.ToString().Should().Be("https://proxy.test/v1/");
    }

    [Fact]
    public async Task CreateEmbeddingsAsync_OnInvalidJsonResponse_ReturnsProviderError()
    {
        // Arrange
        var (client, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/embeddings").Respond("application/json", "not json");

        // Act
        var result = await client.CreateEmbeddingsAsync(
            new OpenAIEmbeddingsRequest { Model = "m", Input = new List<string> { "x" } },
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.ProviderError");
    }

    [Fact]
    public async Task CreateEmbeddingsAsync_OnHttpRequestException_ReturnsProviderError()
    {
        // Arrange
        var (client, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/embeddings").Throw(new HttpRequestException("nope"));

        // Act
        var result = await client.CreateEmbeddingsAsync(
            new OpenAIEmbeddingsRequest { Model = "m", Input = new List<string> { "x" } },
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.ProviderError");
    }

    [Fact]
    public async Task CreateEmbeddingsAsync_WhenCallerCancels_RethrowsCancellation()
    {
        // Arrange
        var (client, handler) = TestFactories.CreateHttpClient();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        handler.When(HttpMethod.Post, "*/embeddings").Throw(new TaskCanceledException("c"));

        // Act
        var act = async () => await client.CreateEmbeddingsAsync(
            new OpenAIEmbeddingsRequest { Model = "m", Input = new List<string> { "x" } },
            cts.Token);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task LogsRequestAndResponse_WhenEnableLoggingTrue()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o => o.EnableLogging = true);
        var logger = new RecordingLogger<OpenAIHttpClient>();
        var client = new OpenAIHttpClient(inner, Options.Create(options), logger);

        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("application/json", """{"id":"x","model":"m","created":0,"choices":[]}""");

        // Act
        await client.CreateChatCompletionAsync(
            new OpenAIChatCompletionRequest
            {
                Model = "m",
                Messages = new List<OpenAIChatMessage> { new() { Role = "user", Content = "hi" } }
            },
            CancellationToken.None);

        // Assert
        logger.Entries.Should().Contain(e => e.Message.Contains("OpenAI request:"));
        logger.Entries.Should().Contain(e => e.Message.Contains("OpenAI response"));
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception), exception));
        private sealed class NullScope : IDisposable { public static readonly NullScope Instance = new(); public void Dispose() { } }
    }
}
