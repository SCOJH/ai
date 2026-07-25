// -----------------------------------------------------------------------
// <copyright file="LiteLLMApiModels.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.LiteLLM.Http.Models;

/// <summary>LiteLLM chat completion request body (OpenAI-compatible).</summary>
internal sealed class LiteLLMChatCompletionRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required List<LiteLLMChatMessage> Messages { get; set; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    [JsonPropertyName("stop")]
    public List<string>? Stop { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("stream_options")]
    public LiteLLMStreamOptions? StreamOptions { get; set; }

    [JsonPropertyName("tools")]
    public List<LiteLLMToolDefinition>? Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }

    [JsonPropertyName("response_format")]
    public LiteLLMResponseFormat? ResponseFormat { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }
}

internal sealed class LiteLLMStreamOptions
{
    [JsonPropertyName("include_usage")]
    public bool IncludeUsage { get; set; }
}

/// <summary>LiteLLM chat message (OpenAI-compatible) — supports tool_calls and tool-result role.</summary>
internal sealed class LiteLLMChatMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<LiteLLMToolCall>? ToolCalls { get; set; }
}

internal sealed class LiteLLMToolDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public required LiteLLMFunctionDefinition Function { get; set; }
}

internal sealed class LiteLLMFunctionDefinition
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    public JsonElement? Parameters { get; set; }
}

internal sealed class LiteLLMToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public LiteLLMToolCallFunction? Function { get; set; }
}

internal sealed class LiteLLMToolCallFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}

internal sealed class LiteLLMResponseFormat
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("json_schema")]
    public LiteLLMJsonSchemaFormat? JsonSchema { get; set; }
}

internal sealed class LiteLLMJsonSchemaFormat
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("schema")]
    public JsonElement Schema { get; set; }

    [JsonPropertyName("strict")]
    public bool? Strict { get; set; }
}

/// <summary>LiteLLM chat completion response.</summary>
internal sealed class LiteLLMChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("choices")]
    public List<LiteLLMChatChoice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public LiteLLMUsage? Usage { get; set; }
}

internal sealed class LiteLLMChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public LiteLLMChatMessage? Message { get; set; }

    [JsonPropertyName("delta")]
    public LiteLLMChatDelta? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

internal sealed class LiteLLMChatDelta
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<LiteLLMToolCall>? ToolCalls { get; set; }
}

internal sealed class LiteLLMUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>LiteLLM streaming SSE chunk.</summary>
internal sealed class LiteLLMStreamChunk
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<LiteLLMChatChoice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public LiteLLMUsage? Usage { get; set; }
}

/// <summary>LiteLLM embeddings request body (OpenAI-compatible).</summary>
internal sealed class LiteLLMEmbeddingsRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("input")]
    public required List<string> Input { get; set; }

    [JsonPropertyName("dimensions")]
    public int? Dimensions { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("encoding_format")]
    public string EncodingFormat { get; set; } = "float";
}

internal sealed class LiteLLMEmbeddingsResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<LiteLLMEmbeddingData> Data { get; set; } = new();

    [JsonPropertyName("usage")]
    public LiteLLMEmbeddingsUsage? Usage { get; set; }
}

internal sealed class LiteLLMEmbeddingData
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = Array.Empty<float>();
}

internal sealed class LiteLLMEmbeddingsUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>LiteLLM list-models response (OpenAI-compatible).</summary>
internal sealed class LiteLLMModelsResponse
{
    [JsonPropertyName("data")]
    public List<LiteLLMModelInfo> Data { get; set; } = new();
}

internal sealed class LiteLLMModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("owned_by")]
    public string? OwnedBy { get; set; }

    [JsonPropertyName("created")]
    public long? Created { get; set; }
}

internal sealed class LiteLLMErrorResponse
{
    [JsonPropertyName("error")]
    public LiteLLMError? Error { get; set; }
}

internal sealed class LiteLLMError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
