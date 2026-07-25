// -----------------------------------------------------------------------
// <copyright file="GeminiIntegrationTests.cs" company="Sassy Solutions">
//     Copyright (c) 2026 Sassy Solutions. Licensed under the MIT License.
//     See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Compendium.Adapters.Gemini.IntegrationTests;

/// <summary>
/// Hits the real Gemini API. Skipped when the API-key env var is not set so unit-only CI stays
/// hermetic. To run locally:
/// <code>
/// export GEMINI_API_KEY=...
/// dotnet test -c Release --filter "FullyQualifiedName~IntegrationTests"
/// </code>
/// </summary>
[Trait("Category", "RequiresGemini")]
public sealed class GeminiIntegrationTests
{
    private const string Model = "gemini-2.0-flash";
    private const string EmbeddingModel = "text-embedding-004";
    private const string EnvVar = "GEMINI_API_KEY";

    private static string? ApiKey => Environment.GetEnvironmentVariable(EnvVar);

    private static IAIProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCompendiumGemini(opt =>
        {
            opt.ApiKey = ApiKey!;
            opt.DefaultModel = Model;
            opt.DefaultEmbeddingModel = EmbeddingModel;
        });
        return services.BuildServiceProvider().GetRequiredService<IAIProvider>();
    }

    [SkippableFact]
    public async Task Chat_BasicRoundTrip_ReturnsAssistantContent()
    {
        Skip.If(string.IsNullOrEmpty(ApiKey), $"{EnvVar} not set");

        var provider = BuildProvider();
        var result = await provider.CompleteAsync(new CompletionRequest
        {
            Model = Model,
            SystemPrompt = "Reply with the single word 'pong'.",
            Messages = new List<Message> { Message.User("ping") },
            MaxTokens = 16
        });

        result.IsSuccess.Should().BeTrue($"completion should succeed; error: {(result.IsFailure ? result.Error.Message : "")}");
        result.Value.Content.Should().NotBeNullOrWhiteSpace();
    }

    [SkippableFact]
    public async Task Embed_SingleInput_ReturnsVector()
    {
        Skip.If(string.IsNullOrEmpty(ApiKey), $"{EnvVar} not set");

        var provider = BuildProvider();
        var result = await provider.EmbedAsync(new EmbeddingRequest
        {
            Model = EmbeddingModel,
            Inputs = new List<string> { "hello" }
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Embeddings.Should().ContainSingle();
        result.Value.Embeddings[0].Vector.Length.Should().BeGreaterThan(0);
    }

    [SkippableFact]
    public async Task Embed_MultipleInputs_ReturnsVectorPerInput()
    {
        Skip.If(string.IsNullOrEmpty(ApiKey), $"{EnvVar} not set");

        var provider = BuildProvider();
        var result = await provider.EmbedAsync(new EmbeddingRequest
        {
            Model = EmbeddingModel,
            Inputs = new List<string> { "hello", "world" }
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Embeddings.Should().HaveCount(2);
    }

    [SkippableFact]
    public async Task Chat_WithTool_EmitsToolCallInvocation()
    {
        Skip.If(string.IsNullOrEmpty(ApiKey), $"{EnvVar} not set");

        var provider = BuildProvider();
        var tools = new List<AgentTool>
        {
            new(
                "get_weather",
                "Returns the current weather for the named city.",
                """{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""")
        };
        var request = new CompletionRequest
        {
            Model = Model,
            Messages = new List<Message> { Message.User("What's the weather in Paris right now?") },
            MaxTokens = 256
        }.WithTools(tools, toolChoice: "auto");

        var result = await provider.CompleteAsync(request);

        result.IsSuccess.Should().BeTrue();
        var calls = result.Value.GetToolCalls();
        calls.Should().NotBeEmpty("the model should request the weather tool");
        calls[0].ToolName.Should().Be("get_weather");
    }

    [SkippableFact]
    public async Task Chat_WithStructuredOutput_ReturnsValidJson()
    {
        Skip.If(string.IsNullOrEmpty(ApiKey), $"{EnvVar} not set");

        var provider = BuildProvider();
        var schema = """
        {
          "type":"object",
          "properties":{ "answer": { "type": "string" } },
          "required":["answer"]
        }
        """;
        var request = new CompletionRequest
        {
            Model = Model,
            SystemPrompt = "You reply with JSON only.",
            Messages = new List<Message> { Message.User("Say hello.") },
            MaxTokens = 64
        }.WithStructuredOutput(schema);

        var result = await provider.CompleteAsync(request);

        result.IsSuccess.Should().BeTrue();
        var act = () => System.Text.Json.JsonDocument.Parse(result.Value.Content);
        act.Should().NotThrow();
    }

    [SkippableFact]
    public async Task Chat_Streaming_YieldsDeltas()
    {
        Skip.If(string.IsNullOrEmpty(ApiKey), $"{EnvVar} not set");

        var provider = BuildProvider();
        var chunks = new List<CompletionChunk>();
        await foreach (var r in provider.StreamCompleteAsync(new CompletionRequest
        {
            Model = Model,
            Messages = new List<Message> { Message.User("Count to 3.") },
            MaxTokens = 32
        }))
        {
            r.IsSuccess.Should().BeTrue();
            chunks.Add(r.Value);
        }

        chunks.Should().NotBeEmpty();
        chunks[^1].IsFinal.Should().BeTrue();
    }
}
