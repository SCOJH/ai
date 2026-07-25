// -----------------------------------------------------------------------
// <copyright file="LiteLLMHttpClientTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.LiteLLM.Http;
using Compendium.Adapters.LiteLLM.Http.Models;
using Compendium.Adapters.LiteLLM.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;

namespace Compendium.Adapters.LiteLLM.Tests.Http;

public class LiteLLMHttpClientTests
{
    [Fact]
    public void Ctor_SetsBearerToken_FromApiKey()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o => o.ApiKey = "plain-bearer");

        // Act
        _ = new LiteLLMHttpClient(inner, Options.Create(options), NullLogger<LiteLLMHttpClient>.Instance);

        // Assert
        inner.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        inner.DefaultRequestHeaders.Authorization!.Parameter.Should().Be("plain-bearer");
    }

    [Fact]
    public void Ctor_PrefersVirtualKeyOverApiKey_OnAuthorizationHeader()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o =>
        {
            o.ApiKey = "plain";
            o.VirtualKey = "virtual-team-key";
        });

        // Act
        _ = new LiteLLMHttpClient(inner, Options.Create(options), NullLogger<LiteLLMHttpClient>.Instance);

        // Assert
        inner.DefaultRequestHeaders.Authorization!.Parameter.Should().Be("virtual-team-key");
    }

    [Fact]
    public void Ctor_OmitsAuthorizationHeader_WhenNoCredentialConfigured()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o =>
        {
            o.ApiKey = string.Empty;
            o.VirtualKey = null;
        });

        // Act
        _ = new LiteLLMHttpClient(inner, Options.Create(options), NullLogger<LiteLLMHttpClient>.Instance);

        // Assert — many local LiteLLM proxies run with no auth at all.
        inner.DefaultRequestHeaders.Contains("Authorization").Should().BeFalse();
    }

    [Fact]
    public void Ctor_DoesNotOverridePreSetBaseAddress()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler) { BaseAddress = new Uri("https://proxy.test/v1/") };
        var options = TestFactories.DefaultOptions();

        // Act
        _ = new LiteLLMHttpClient(inner, Options.Create(options), NullLogger<LiteLLMHttpClient>.Instance);

        // Assert
        inner.BaseAddress!.ToString().Should().Be("https://proxy.test/v1/");
    }

    [Fact]
    public void Ctor_ForwardsPassThroughHeaders()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o =>
        {
            o.PassThroughHeaders["x-litellm-cache-key"] = "user-42-prompt-7";
            o.PassThroughHeaders["x-litellm-tags"] = "env=staging,team=ai";
        });

        // Act
        _ = new LiteLLMHttpClient(inner, Options.Create(options), NullLogger<LiteLLMHttpClient>.Instance);

        // Assert
        inner.DefaultRequestHeaders.GetValues("x-litellm-cache-key").Should().Contain("user-42-prompt-7");
        inner.DefaultRequestHeaders.GetValues("x-litellm-tags").Should().Contain("env=staging,team=ai");
    }

    [Fact]
    public void Ctor_PassThroughHeader_DoesNotOverrideAuthorization()
    {
        // Arrange — even if a user tries to override Authorization via pass-through,
        // the configured ApiKey / VirtualKey wins.
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o =>
        {
            o.ApiKey = "real-key";
            o.PassThroughHeaders["Authorization"] = "Bearer hacked";
        });

        // Act
        _ = new LiteLLMHttpClient(inner, Options.Create(options), NullLogger<LiteLLMHttpClient>.Instance);

        // Assert
        inner.DefaultRequestHeaders.Authorization!.Parameter.Should().Be("real-key");
    }

    [Fact]
    public void Ctor_PassThroughHeader_IgnoresBlankNamesOrNullValues()
    {
        // Arrange — a blank header name should be silently skipped (a runtime
        // HttpHeaders.Contains/Add with a blank name would normally throw FormatException).
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o =>
        {
            o.PassThroughHeaders["   "] = "should-be-skipped";
            o.PassThroughHeaders["x-litellm-tags"] = "real";
        });

        // Act
        var act = () => new LiteLLMHttpClient(inner, Options.Create(options), NullLogger<LiteLLMHttpClient>.Instance);

        // Assert — construction must not throw and the well-formed header must still be applied.
        act.Should().NotThrow();
        inner.DefaultRequestHeaders.GetValues("x-litellm-tags").Should().Contain("real");
    }

    [Fact]
    public async Task CreateChatCompletionAsync_SendsConfiguredPassThroughHeaders_OnOutboundRequest()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o =>
        {
            o.PassThroughHeaders["x-litellm-cache-key"] = "abc-123";
        });
        var sut = new LiteLLMHttpClient(inner, Options.Create(options), NullLogger<LiteLLMHttpClient>.Instance);

        handler.Expect(HttpMethod.Post, "*/chat/completions")
            .WithHeaders("x-litellm-cache-key", "abc-123")
            .Respond("application/json", """{"id":"x","model":"m","created":0,"choices":[]}""");

        // Act
        var result = await sut.CreateChatCompletionAsync(
            new LiteLLMChatCompletionRequest
            {
                Model = "openai/gpt-4o-mini",
                Messages = new List<LiteLLMChatMessage> { new() { Role = "user", Content = "hi" } }
            },
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        handler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task CreateChatCompletionAsync_SerializesProviderPrefixedModelId_Unchanged()
    {
        // Arrange — LiteLLM uses provider-prefixed ids like "anthropic/claude-sonnet-4".
        var (sut, handler) = TestFactories.CreateHttpClient();
        string? capturedBody = null;
        handler.When(HttpMethod.Post, "*/chat/completions")
            .With(req =>
            {
                capturedBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return true;
            })
            .Respond("application/json", """{"id":"x","model":"anthropic/claude-sonnet-4","created":0,"choices":[]}""");

        // Act
        await sut.CreateChatCompletionAsync(
            new LiteLLMChatCompletionRequest
            {
                Model = "anthropic/claude-sonnet-4",
                Messages = new List<LiteLLMChatMessage> { new() { Role = "user", Content = "hi" } }
            },
            CancellationToken.None);

        // Assert
        capturedBody.Should().NotBeNull();
        capturedBody!.Should().Contain("\"model\":\"anthropic/claude-sonnet-4\"");
    }

    [Fact]
    public async Task CreateEmbeddingsAsync_OnInvalidJsonResponse_ReturnsProviderError()
    {
        // Arrange
        var (client, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/embeddings").Respond("application/json", "not json");

        // Act
        var result = await client.CreateEmbeddingsAsync(
            new LiteLLMEmbeddingsRequest { Model = "m", Input = new List<string> { "x" } },
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
            new LiteLLMEmbeddingsRequest { Model = "m", Input = new List<string> { "x" } },
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
            new LiteLLMEmbeddingsRequest { Model = "m", Input = new List<string> { "x" } },
            cts.Token);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task CreateChatCompletionAsync_LogsRequestAndResponse_WhenEnableLoggingTrue()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o => o.EnableLogging = true);
        var logger = new TestFactories.RecordingLogger<LiteLLMHttpClient>();
        var client = new LiteLLMHttpClient(inner, Options.Create(options), logger);

        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("application/json", """{"id":"x","model":"m","created":0,"choices":[]}""");

        // Act
        await client.CreateChatCompletionAsync(
            new LiteLLMChatCompletionRequest
            {
                Model = "m",
                Messages = new List<LiteLLMChatMessage> { new() { Role = "user", Content = "hi" } }
            },
            CancellationToken.None);

        // Assert
        logger.Entries.Should().Contain(e => e.Message.Contains("LiteLLM request"));
        logger.Entries.Should().Contain(e => e.Message.Contains("LiteLLM response"));
    }

    [Fact]
    public async Task CreateEmbeddingsAsync_LogsRequest_WhenEnableLoggingTrue()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        var inner = new HttpClient(handler);
        var options = TestFactories.DefaultOptions(o => o.EnableLogging = true);
        var logger = new TestFactories.RecordingLogger<LiteLLMHttpClient>();
        var client = new LiteLLMHttpClient(inner, Options.Create(options), logger);

        handler.When(HttpMethod.Post, "*/embeddings")
            .Respond("application/json", """{"model":"m","data":[],"usage":{"prompt_tokens":0,"total_tokens":0}}""");

        // Act
        await client.CreateEmbeddingsAsync(
            new LiteLLMEmbeddingsRequest { Model = "m", Input = new List<string> { "x" } },
            CancellationToken.None);

        // Assert
        logger.Entries.Should().Contain(e => e.Message.Contains("LiteLLM embeddings request"));
    }

    [Fact]
    public async Task ListModelsAsync_OnSuccess_ReturnsModelsList()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        var json = """
        {
          "data": [
            { "id": "openai/gpt-4o", "owned_by": "openai" },
            { "id": "anthropic/claude-sonnet-4", "owned_by": "anthropic" }
          ]
        }
        """;
        handler.When(HttpMethod.Get, "*/models").Respond("application/json", json);

        // Act
        var result = await sut.ListModelsAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Id.Should().Be("openai/gpt-4o");
        result.Value[1].Id.Should().Be("anthropic/claude-sonnet-4");
    }

    [Fact]
    public async Task ListModelsAsync_OnUnexpectedException_ReturnsProviderError()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Get, "*/models").Throw(new InvalidOperationException("dns failed"));

        // Act
        var result = await sut.ListModelsAsync(CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.ProviderError");
        result.Error.Message.Should().Contain("dns failed");
    }

    [Fact]
    public async Task ListModelsAsync_WhenCallerCancels_PropagatesTaskCanceled()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        handler.When(HttpMethod.Get, "*/models").Throw(new TaskCanceledException("cancelled"));

        // Act
        var act = async () => await sut.ListModelsAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
