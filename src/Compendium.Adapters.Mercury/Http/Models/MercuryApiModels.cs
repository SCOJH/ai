// -----------------------------------------------------------------------
// <copyright file="MercuryApiModels.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Mercury.Http.Models;

/// <summary>
/// Mercury chat-completion request (OpenAI-compatible).
/// </summary>
internal sealed class MercuryCompletionRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required List<MercuryMessage> Messages { get; set; }

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

    [JsonPropertyName("tools")]
    public List<MercuryTool>? Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }
}

/// <summary>
/// Mercury message (chat-completion compatible).
/// </summary>
internal sealed class MercuryMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<MercuryToolCall>? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
}

/// <summary>
/// Tool definition supplied with the request.
/// </summary>
internal sealed class MercuryTool
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public required MercuryFunctionDef Function { get; set; }
}

/// <summary>
/// Function-tool schema description.
/// </summary>
internal sealed class MercuryFunctionDef
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    public JsonElement? Parameters { get; set; }
}

/// <summary>
/// Tool-call emitted by the model.
/// </summary>
internal sealed class MercuryToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public MercuryFunctionCall? Function { get; set; }
}

/// <summary>
/// Function name + JSON argument string emitted by the model.
/// </summary>
internal sealed class MercuryFunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>
/// Chat-completion response.
/// </summary>
internal sealed class MercuryCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("choices")]
    public List<MercuryChoice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public MercuryUsage? Usage { get; set; }
}

/// <summary>
/// One choice in a completion response.
/// </summary>
internal sealed class MercuryChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public MercuryMessage? Message { get; set; }

    [JsonPropertyName("delta")]
    public MercuryDelta? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

/// <summary>
/// Streaming delta segment.
/// </summary>
internal sealed class MercuryDelta
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<MercuryToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// Token-usage statistics.
/// </summary>
internal sealed class MercuryUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// Streaming chunk envelope.
/// </summary>
internal sealed class MercuryStreamChunk
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<MercuryChoice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public MercuryUsage? Usage { get; set; }
}

/// <summary>
/// Models-list response envelope.
/// </summary>
internal sealed class MercuryModelsResponse
{
    [JsonPropertyName("data")]
    public List<MercuryModel> Data { get; set; } = new();
}

/// <summary>
/// Single model entry from the models-list endpoint.
/// </summary>
internal sealed class MercuryModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("context_length")]
    public int? ContextLength { get; set; }

    [JsonPropertyName("max_output_length")]
    public int? MaxOutputLength { get; set; }

    [JsonPropertyName("input_modalities")]
    public List<string>? InputModalities { get; set; }

    [JsonPropertyName("supported_features")]
    public List<string>? SupportedFeatures { get; set; }

    [JsonPropertyName("pricing")]
    public MercuryPricing? Pricing { get; set; }
}

/// <summary>
/// Pricing block reported by the models endpoint (per-token amounts as strings).
/// </summary>
internal sealed class MercuryPricing
{
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    [JsonPropertyName("completion")]
    public string? Completion { get; set; }
}

/// <summary>
/// OpenAI-style error envelope.
/// </summary>
internal sealed class MercuryErrorResponse
{
    [JsonPropertyName("error")]
    public MercuryError? Error { get; set; }
}

/// <summary>
/// OpenAI-style error details.
/// </summary>
internal sealed class MercuryError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
