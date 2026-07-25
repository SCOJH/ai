// -----------------------------------------------------------------------
// <copyright file="MercuryHttpClientTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using Compendium.Adapters.Mercury.Http;
using Compendium.Adapters.Mercury.Http.Models;
using Compendium.Adapters.Mercury.Tests.TestSupport;

namespace Compendium.Adapters.Mercury.Tests.Http;

/// <summary>
/// Unit tests for <see cref="MercuryHttpClient"/>. HTTP transport is mocked with RichardSzalay.MockHttp.
/// </summary>
public class MercuryHttpClientTests
{
    private static MercuryCompletionRequest BuildRequest(string model = "mercury") =>
        new()
        {
            Model = model,
            Messages = new List<MercuryMessage>
            {
                new() { Role = "user", Content = "Hello" },
            },
        };

    [Fact]
    public void Constructor_ConfiguresBearerAuthorizationHeader()
    {
        // Arrange
        var (sut, _) = TestFactories.CreateHttpClient(o => o.ApiKey = "sk-mercury-mykey");

        // Act
        var headers = GetUnderlyingHttpClient(sut).DefaultRequestHeaders;

        // Assert
        headers.Authorization.Should().NotBeNull();
        headers.Authorization!.Scheme.Should().Be("Bearer");
        headers.Authorization!.Parameter.Should().Be("sk-mercury-mykey");
    }

    [Fact]
    public void Constructor_WhenApiKeyIsEmpty_DoesNotAddAuthorizationHeader()
    {
        // Arrange
        var (sut, _) = TestFactories.CreateHttpClient(o => o.ApiKey = string.Empty);

        // Act
        var headers = GetUnderlyingHttpClient(sut).DefaultRequestHeaders;

        // Assert
        headers.Authorization.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenBaseAddressAlreadySet_DoesNotOverride()
    {
        // Arrange — simulate a typed-client whose factory has already configured BaseAddress.
        var handler = new MockHttpMessageHandler();
        var options = TestFactories.DefaultOptions(o => o.BaseUrl = "https://will-not-be-used.example.com");
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://preset.example.com/v1/"),
        };

        // Act
        var sut = new MercuryHttpClient(
            httpClient,
            Options.Create(options),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<MercuryHttpClient>.Instance);

        // Assert
        GetUnderlyingHttpClient(sut).BaseAddress.Should().Be(new Uri("https://preset.example.com/v1/"));
    }

    [Fact]
    public async Task CreateCompletionAsync_OnSuccess_ReturnsParsedResponse()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        var responseJson = """
        {
          "id": "mer-abc",
          "model": "mercury",
          "created": 1730000000,
          "choices": [
            {
              "index": 0,
              "message": { "role": "assistant", "content": "Hi!" },
              "finish_reason": "stop"
            }
          ],
          "usage": { "prompt_tokens": 10, "completion_tokens": 4, "total_tokens": 14 }
        }
        """;
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("application/json", responseJson);

        // Act
        var result = await sut.CreateCompletionAsync(BuildRequest(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("mer-abc");
        result.Value.Model.Should().Be("mercury");
        result.Value.Choices.Should().ContainSingle();
        result.Value.Choices[0].Message!.Content.Should().Be("Hi!");
        result.Value.Usage!.PromptTokens.Should().Be(10);
        result.Value.Usage!.CompletionTokens.Should().Be(4);
    }

    [Fact]
    public async Task CreateCompletionAsync_WhenLoggingEnabled_LogsRequestAndResponse()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("application/json", """{"id":"x","model":"m","created":0,"choices":[]}""");
        var options = TestFactories.DefaultOptions(o => o.EnableLogging = true);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(options.BaseUrl) };
        var logger = new TestFactories.RecordingLogger<MercuryHttpClient>();
        var sut = new MercuryHttpClient(httpClient, Options.Create(options), logger);

        // Act
        var result = await sut.CreateCompletionAsync(BuildRequest(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().Contain(e => e.Message.Contains("Mercury request"));
        logger.Entries.Should().Contain(e => e.Message.Contains("Mercury response"));
    }

    [Fact]
    public async Task CreateCompletionAsync_WhenResponseIsNullJson_ReturnsProviderError()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("application/json", "null");

        // Act
        var result = await sut.CreateCompletionAsync(BuildRequest(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.ProviderError");
        result.Error.Message.Should().Contain("Empty response");
    }

    [Fact]
    public async Task CreateCompletionAsync_WhenResponseIsInvalidJson_ReturnsProviderError()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("application/json", "this is not json");

        // Act
        var result = await sut.CreateCompletionAsync(BuildRequest(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.ProviderError");
        result.Error.Message.Should().Contain("Invalid response format");
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "AI.InvalidApiKey")]
    [InlineData(HttpStatusCode.TooManyRequests, "AI.RateLimitExceeded")]
    [InlineData(HttpStatusCode.PaymentRequired, "AI.InsufficientCredits")]
    [InlineData(HttpStatusCode.NotFound, "AI.ModelNotFound")]
    [InlineData(HttpStatusCode.InternalServerError, "AI.ProviderError")]
    public async Task CreateCompletionAsync_OnErrorStatus_MapsToTypedError(HttpStatusCode status, string expectedCode)
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond(status, "application/json", """{"error":{"code":"E1","message":"boom","type":"x"}}""");

        // Act
        var result = await sut.CreateCompletionAsync(BuildRequest(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(expectedCode);
    }

    [Fact]
    public async Task CreateCompletionAsync_OnErrorStatus_WithNonJsonBody_ReturnsProviderErrorWithRawBody()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond(HttpStatusCode.BadGateway, "text/plain", "raw body text");

        // Act
        var result = await sut.CreateCompletionAsync(BuildRequest(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.ProviderError");
        result.Error.Message.Should().Contain("raw body text");
    }

    [Fact]
    public async Task CreateCompletionAsync_OnErrorStatus_WithMissingErrorField_FallsBackToRawBody()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond(HttpStatusCode.InternalServerError, "application/json", "{\"foo\":\"bar\"}");

        // Act
        var result = await sut.CreateCompletionAsync(BuildRequest(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.ProviderError");
        result.Error.Message.Should().Contain("foo");
    }

    [Fact]
    public async Task CreateCompletionAsync_OnHttpRequestException_ReturnsProviderError()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Throw(new HttpRequestException("connection refused"));

        // Act
        var result = await sut.CreateCompletionAsync(BuildRequest(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.ProviderError");
        result.Error.Message.Should().Contain("connection refused");
    }

    [Fact]
    public async Task CreateCompletionAsync_OnTimeoutCanceledInternally_ReturnsTimeoutError()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient(o => o.TimeoutSeconds = 7);
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Throw(new TaskCanceledException("timeout"));

        // Act
        var result = await sut.CreateCompletionAsync(BuildRequest(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.Timeout");
        result.Error.Message.Should().Contain("7");
    }

    [Fact]
    public async Task CreateCompletionAsync_WhenCallerCancels_PropagatesTaskCanceledException()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Throw(new TaskCanceledException("cancelled"));

        // Act
        var act = async () => await sut.CreateCompletionAsync(BuildRequest(), cts.Token);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task ListModelsAsync_OnSuccess_ReturnsModelsList()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        var json = """
        {
          "data": [
            { "id": "mercury", "name": "Inception: Mercury", "context_length": 128000, "max_output_length": 32000 },
            { "id": "mercury-coder", "context_length": 128000, "max_output_length": 32000 }
          ]
        }
        """;
        handler.When(HttpMethod.Get, "*/models")
            .Respond("application/json", json);

        // Act
        var result = await sut.ListModelsAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Id.Should().Be("mercury");
        result.Value[1].Id.Should().Be("mercury-coder");
    }

    [Fact]
    public async Task ListModelsAsync_OnNon2xx_ReturnsMappedError()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Get, "*/models")
            .Respond(HttpStatusCode.Unauthorized, "application/json", "{\"error\":{\"message\":\"bad key\"}}");

        // Act
        var result = await sut.ListModelsAsync(CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.InvalidApiKey");
    }

    [Fact]
    public async Task ListModelsAsync_OnUnexpectedException_ReturnsProviderError()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Get, "*/models")
            .Throw(new InvalidOperationException("DNS failed"));

        // Act
        var result = await sut.ListModelsAsync(CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.ProviderError");
        result.Error.Message.Should().Contain("DNS failed");
    }

    [Fact]
    public async Task ListModelsAsync_WhenCallerCancels_PropagatesTaskCanceledException()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        handler.When(HttpMethod.Get, "*/models")
            .Throw(new TaskCanceledException("cancelled"));

        // Act
        var act = async () => await sut.ListModelsAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task CreateCompletionStreamAsync_OnSuccess_YieldsParsedChunks()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        var stream = string.Join(
            "\n",
            "data: {\"id\":\"c1\",\"model\":\"mercury\",\"choices\":[{\"delta\":{\"content\":\"Hel\"}}]}",
            string.Empty,
            "data: {\"id\":\"c1\",\"model\":\"mercury\",\"choices\":[{\"delta\":{\"content\":\"lo\"}}]}",
            "data: {\"id\":\"c1\",\"model\":\"mercury\",\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}]}",
            "data: [DONE]",
            string.Empty);
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("text/event-stream", stream);

        // Act
        var chunks = new List<MercuryStreamChunk>();
        await foreach (var c in sut.CreateCompletionStreamAsync(BuildRequest(), CancellationToken.None))
        {
            c.IsSuccess.Should().BeTrue();
            chunks.Add(c.Value);
        }

        // Assert
        chunks.Should().HaveCount(3);
        chunks[0].Choices[0].Delta!.Content.Should().Be("Hel");
        chunks[1].Choices[0].Delta!.Content.Should().Be("lo");
        chunks[2].Choices[0].FinishReason.Should().Be("stop");
    }

    [Fact]
    public async Task CreateCompletionStreamAsync_OnNon2xxStatus_YieldsSingleFailureAndStops()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond(HttpStatusCode.TooManyRequests, "application/json", "{\"error\":{\"message\":\"slow down\"}}");

        // Act
        var results = new List<Result<MercuryStreamChunk>>();
        await foreach (var r in sut.CreateCompletionStreamAsync(BuildRequest(), CancellationToken.None))
        {
            results.Add(r);
        }

        // Assert
        results.Should().ContainSingle();
        results[0].IsFailure.Should().BeTrue();
        results[0].Error.Code.Should().Be("AI.RateLimitExceeded");
    }

    [Fact]
    public async Task CreateCompletionStreamAsync_SkipsBlankLines_NonDataLines_AndUnparseableData()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        var stream = string.Join(
            "\n",
            string.Empty,
            ": comment line",
            "event: message",
            "data: not-json",
            "data: {\"id\":\"c1\",\"model\":\"m\",\"choices\":[{\"delta\":{\"content\":\"ok\"}}]}",
            "data: [DONE]",
            string.Empty);
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("text/event-stream", stream);

        // Act
        var chunks = new List<MercuryStreamChunk>();
        await foreach (var c in sut.CreateCompletionStreamAsync(BuildRequest(), CancellationToken.None))
        {
            c.IsSuccess.Should().BeTrue();
            chunks.Add(c.Value);
        }

        // Assert
        chunks.Should().ContainSingle();
        chunks[0].Choices[0].Delta!.Content.Should().Be("ok");
    }

    [Fact]
    public async Task CreateCompletionStreamAsync_StopsWhenCancellationRequested()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        var stream = string.Join(
            "\n",
            "data: {\"id\":\"c1\",\"model\":\"m\",\"choices\":[{\"delta\":{\"content\":\"a\"}}]}",
            "data: {\"id\":\"c1\",\"model\":\"m\",\"choices\":[{\"delta\":{\"content\":\"b\"}}]}",
            "data: {\"id\":\"c1\",\"model\":\"m\",\"choices\":[{\"delta\":{\"content\":\"c\"}}]}",
            "data: [DONE]",
            string.Empty);
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("text/event-stream", stream);

        using var cts = new CancellationTokenSource();
        var chunks = new List<MercuryStreamChunk>();

        // Act
        await foreach (var c in sut.CreateCompletionStreamAsync(BuildRequest(), cts.Token))
        {
            if (c.IsSuccess)
            {
                chunks.Add(c.Value);
            }

            cts.Cancel();
        }

        // Assert
        chunks.Should().HaveCountGreaterThan(0);
        chunks.Should().HaveCountLessThan(4);
    }

    [Fact]
    public async Task CreateCompletionAsync_WithToolsInRequest_SerialisesToolDefinitions()
    {
        // Arrange — exercises the tool-calling round-trip the Mercury API advertises
        // (the /v1/models payload lists "tools" in supported_features for mercury / mercury-2 / mercury-coder).
        var (sut, handler) = TestFactories.CreateHttpClient();
        string? body = null;
        var parametersJson = JsonDocument.Parse(
            """{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""");

        var request = new MercuryCompletionRequest
        {
            Model = "mercury",
            Messages = new List<MercuryMessage> { new() { Role = "user", Content = "weather?" } },
            Tools = new List<MercuryTool>
            {
                new()
                {
                    Function = new MercuryFunctionDef
                    {
                        Name = "get_weather",
                        Description = "Fetch the current weather for a city",
                        Parameters = parametersJson.RootElement,
                    },
                },
            },
            ToolChoice = "auto",
        };

        var responseJson = """
        {
          "id": "tc-1",
          "model": "mercury",
          "created": 0,
          "choices": [
            {
              "index": 0,
              "finish_reason": "tool_calls",
              "message": {
                "role": "assistant",
                "tool_calls": [
                  { "id": "call_1", "type": "function", "function": { "name": "get_weather", "arguments": "{\"city\":\"Paris\"}" } }
                ]
              }
            }
          ]
        }
        """;
        handler.When(HttpMethod.Post, "*/chat/completions")
            .With(req =>
            {
                body = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return true;
            })
            .Respond("application/json", responseJson);

        // Act
        var result = await sut.CreateCompletionAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        body.Should().Contain("\"tools\":");
        body.Should().Contain("\"name\":\"get_weather\"");
        body.Should().Contain("\"tool_choice\":\"auto\"");

        result.Value.Choices.Should().ContainSingle();
        var message = result.Value.Choices[0].Message!;
        message.ToolCalls.Should().NotBeNull();
        message.ToolCalls!.Should().ContainSingle();
        message.ToolCalls![0].Id.Should().Be("call_1");
        message.ToolCalls![0].Function!.Name.Should().Be("get_weather");
        message.ToolCalls![0].Function!.Arguments.Should().Contain("Paris");
    }

    [Fact]
    public async Task CreateCompletionStreamAsync_NullChunkAfterDeserialization_IsSkipped()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        var stream = string.Join(
            "\n",
            "data: null",
            "data: {\"id\":\"c1\",\"model\":\"m\",\"choices\":[{\"delta\":{\"content\":\"x\"}}]}",
            "data: [DONE]",
            string.Empty);
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("text/event-stream", stream);

        // Act
        var chunks = new List<MercuryStreamChunk>();
        await foreach (var c in sut.CreateCompletionStreamAsync(BuildRequest(), CancellationToken.None))
        {
            chunks.Add(c.Value);
        }

        // Assert
        chunks.Should().ContainSingle();
        chunks[0].Choices[0].Delta!.Content.Should().Be("x");
    }

    [Fact]
    public async Task CreateCompletionStreamAsync_WhenDeltaCarriesToolCall_DeserialisesIt()
    {
        // Arrange
        var (sut, handler) = TestFactories.CreateHttpClient();
        var stream = string.Join(
            "\n",
            "data: {\"id\":\"c1\",\"model\":\"mercury\",\"choices\":[{\"delta\":{\"tool_calls\":[{\"id\":\"call_1\",\"type\":\"function\",\"function\":{\"name\":\"f\",\"arguments\":\"{}\"}}]}}]}",
            "data: {\"id\":\"c1\",\"model\":\"mercury\",\"choices\":[{\"delta\":{},\"finish_reason\":\"tool_calls\"}]}",
            "data: [DONE]",
            string.Empty);
        handler.When(HttpMethod.Post, "*/chat/completions")
            .Respond("text/event-stream", stream);

        // Act
        var chunks = new List<MercuryStreamChunk>();
        await foreach (var c in sut.CreateCompletionStreamAsync(BuildRequest(), CancellationToken.None))
        {
            c.IsSuccess.Should().BeTrue();
            chunks.Add(c.Value);
        }

        // Assert
        chunks.Should().HaveCount(2);
        chunks[0].Choices[0].Delta!.ToolCalls.Should().NotBeNull();
        chunks[0].Choices[0].Delta!.ToolCalls!.Should().ContainSingle();
        chunks[0].Choices[0].Delta!.ToolCalls![0].Function!.Name.Should().Be("f");
        chunks[1].Choices[0].FinishReason.Should().Be("tool_calls");
    }

    /// <summary>
    /// Reflection helper that exposes the wrapped <see cref="HttpClient"/> so we can inspect
    /// default headers under test.
    /// </summary>
    private static HttpClient GetUnderlyingHttpClient(MercuryHttpClient sut)
    {
        var field = typeof(MercuryHttpClient)
            .GetField("_httpClient", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field.Should().NotBeNull();
        return (HttpClient)field!.GetValue(sut)!;
    }
}
