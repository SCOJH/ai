// -----------------------------------------------------------------------
// <copyright file="MistralStructuredOutputExtensions.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Mistral.StructuredOutputs;

/// <summary>
/// Ergonomic helpers for opting a completion request into Mistral's
/// <c>response_format</c> mode. Mistral supports plain <c>json_object</c> mode on
/// every chat model, and JSON-schema mode on newer Large / Small models.
/// </summary>
public static class MistralStructuredOutputExtensions
{
    /// <summary>Key for the JSON-schema payload.</summary>
    public const string SchemaKey = "mistral.response_format.json_schema";

    /// <summary>Key for the schema name.</summary>
    public const string SchemaNameKey = "mistral.response_format.name";

    /// <summary>Key for strict mode (defaults to true when a schema is supplied).</summary>
    public const string StrictKey = "mistral.response_format.strict";

    /// <summary>Marker key requesting plain <c>json_object</c> mode (no schema).</summary>
    public const string JsonModeKey = "mistral.response_format.json_object";

    /// <summary>
    /// Forces the model to emit JSON conforming to <paramref name="schemaJson"/>.
    /// </summary>
    /// <param name="request">The request to clone.</param>
    /// <param name="schemaJson">JSON-schema document.</param>
    /// <param name="schemaName">Human-readable schema identifier surfaced to Mistral.</param>
    /// <param name="strict">Whether Mistral should reject non-conforming outputs.</param>
    public static CompletionRequest WithStructuredOutput(
        this CompletionRequest request,
        string schemaJson,
        string schemaName = "response",
        bool strict = true)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        var dict = new Dictionary<string, object>(request.AdditionalParameters ?? new Dictionary<string, object>())
        {
            [SchemaKey] = schemaJson,
            [SchemaNameKey] = schemaName,
            [StrictKey] = strict
        };
        return request with { AdditionalParameters = dict };
    }

    /// <summary>
    /// Forces the model to emit valid JSON (without a schema constraint).
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
