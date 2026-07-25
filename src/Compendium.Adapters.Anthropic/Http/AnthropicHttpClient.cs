// -----------------------------------------------------------------------
// <copyright file="AnthropicHttpClient.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

using System.Net;
using System.Text;
using Compendium.Adapters.Anthropic.Configuration;
using Compendium.Adapters.Anthropic.Http.Models;

namespace Compendium.Adapters.Anthropic.Http;

/// <summary>
/// Thin typed-HttpClient wrapper around the Anthropic Messages API. Handles
/// authentication, error mapping, JSON (de)serialisation, and SSE streaming.
/// </summary>
internal sealed class AnthropicHttpClient
{
    private const string MessagesPath = "/v1/messages";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    private readonly HttpClient _httpClient;
    private readonly AnthropicOptions _options;
    private readonly ILogger<AnthropicHttpClient> _logger;

    /// <summary>
    /// Creates a new <see cref="AnthropicHttpClient"/>.
    /// </summary>
    public AnthropicHttpClient(
        HttpClient httpClient,
        IOptions<AnthropicOptions> options,
        ILogger<AnthropicHttpClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("anthropic-version"))
        {
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", _options.AnthropicVersion);
        }
    }

    /// <summary>
    /// POSTs a non-streaming <c>messages</c> request.
    /// </summary>
    public async Task<Result<AnthropicMessagesResponse>> CreateMessageAsync(
        AnthropicMessagesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (_options.EnableLogging)
            {
                _logger.LogDebug("Anthropic request: {Request}", json);
            }

            var response = await _httpClient.PostAsync(MessagesPath, content, cancellationToken);

            return await HandleResponseAsync<AnthropicMessagesResponse>(response, cancellationToken);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Anthropic request timed out");
            return Result.Failure<AnthropicMessagesResponse>(
                AIErrors.Timeout(TimeSpan.FromSeconds(_options.TimeoutSeconds)));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error communicating with Anthropic");
            return Result.Failure<AnthropicMessagesResponse>(AIErrors.ProviderError(ex.Message));
        }
    }

    /// <summary>
    /// POSTs a streaming <c>messages</c> request and yields SSE events as they arrive.
    /// </summary>
    public async IAsyncEnumerable<Result<AnthropicStreamEvent>> CreateMessageStreamAsync(
        AnthropicMessagesRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;
        Stream? stream = null;

        try
        {
            var json = JsonSerializer.Serialize(request, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (_options.EnableLogging)
            {
                _logger.LogDebug("Anthropic stream request: {Request}", json);
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, MessagesPath)
            {
                Content = content,
            };

            response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ParseErrorAsync(response, cancellationToken);
                yield return Result.Failure<AnthropicStreamEvent>(error);
                yield break;
            }

            stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (!line.StartsWith("data: ", StringComparison.Ordinal))
                {
                    continue;
                }

                var data = line[6..];

                AnthropicStreamEvent? evt;
                try
                {
                    evt = JsonSerializer.Deserialize<AnthropicStreamEvent>(data, JsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse Anthropic stream chunk: {Data}", data);
                    continue;
                }

                if (evt is null)
                {
                    continue;
                }

                yield return Result.Success(evt);

                if (string.Equals(evt.Type, "message_stop", StringComparison.Ordinal))
                {
                    yield break;
                }
            }
        }
        finally
        {
            stream?.Dispose();
            response?.Dispose();
        }
    }

    private async Task<Result<T>> HandleResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (_options.EnableLogging)
        {
            _logger.LogDebug(
                "Anthropic response ({StatusCode}): {Content}",
                response.StatusCode,
                content);
        }

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(content, JsonOptions);
                return result is not null
                    ? Result.Success(result)
                    : Result.Failure<T>(AIErrors.ProviderError("Empty response from provider"));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialise Anthropic response");
                return Result.Failure<T>(AIErrors.ProviderError("Invalid response format"));
            }
        }

        var error = await ParseErrorAsync(response, cancellationToken);
        return Result.Failure<T>(error);
    }

    private static async Task<Error> ParseErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        string? message = null;
        string? code = null;

        try
        {
            var parsed = JsonSerializer.Deserialize<AnthropicErrorResponse>(content, JsonOptions);
            message = parsed?.Error?.Message;
            code = parsed?.Error?.Type;
        }
        catch (JsonException)
        {
            // Fallback to raw content below.
        }

        message ??= string.IsNullOrWhiteSpace(content) ? response.ReasonPhrase ?? "Unknown error" : content;

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => AIErrors.InvalidApiKey(),
            HttpStatusCode.PaymentRequired => AIErrors.InsufficientCredits(),
            HttpStatusCode.TooManyRequests => AIErrors.RateLimitExceeded(),
            HttpStatusCode.NotFound => AIErrors.ModelNotFound(message),
            _ => AIErrors.ProviderError(message, code),
        };
    }
}
