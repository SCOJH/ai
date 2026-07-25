// -----------------------------------------------------------------------
// <copyright file="GeminiStructuredOutputExtensions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Gemini.StructuredOutputs;

/// <summary>
/// Ergonomic helpers for opting a completion request into Gemini's
/// <c>responseMimeType</c> / <c>responseSchema</c> structured-output mode.
/// </summary>
public static class GeminiStructuredOutputExtensions
{
    /// <summary>Key for the JSON-schema payload.</summary>
    public const string SchemaKey = "gemini.response_schema";

    /// <summary>Marker key requesting plain JSON mode (no schema).</summary>
    public const string JsonModeKey = "gemini.response_mime_type.json";

    /// <summary>
    /// Forces the model to emit JSON conforming to <paramref name="schemaJson"/>.
    /// </summary>
    /// <param name="request">The request to clone.</param>
    /// <param name="schemaJson">JSON-schema document.</param>
    public static CompletionRequest WithStructuredOutput(
        this CompletionRequest request,
        string schemaJson)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaJson);

        var dict = new Dictionary<string, object>(request.AdditionalParameters ?? new Dictionary<string, object>())
        {
            [SchemaKey] = schemaJson
        };
        return request with { AdditionalParameters = dict };
    }

    /// <summary>
    /// Forces the model to emit valid JSON (without a schema constraint) via
    /// <c>responseMimeType: "application/json"</c>.
    /// </summary>
    public static CompletionRequest WithJsonMode(this CompletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var dict = new Dictionary<string, object>(request.AdditionalParameters ?? new Dictionary<string, object>())
        {
            [JsonModeKey] = true
        };
        return request with { AdditionalParameters = dict };
    }
}
