# Migration provenance — SCOJH/ai domain repo

Assembled 2026-07-25 per ADR-0007 (repo-per-domain topology). Every component was extracted from its
source repository's `origin/main` via `git archive` (sources untouched). SHAs below are the source HEAD
(`origin/main`) at extraction time.

| Component | Source repo | Source HEAD SHA | Notes |
|---|---|---|---|
| `src/Compendium.Abstractions.AI` + `tests/Unit/Compendium.Abstractions.AI.Tests` | `sassy-solutions/compendium` (framework), subtrees `src/Abstractions/Compendium.Abstractions.AI` + `tests/Unit/Compendium.Abstractions.AI.Tests` | `792dd626496f69a4f7c86ce79f48795e6aba7e34` | `Compendium.Core` ProjectReference → PackageReference; `<IsPackable>true</IsPackable>` added (framework set it via its own Build.props). Test files `Agents/ReActActionParserTests.cs` + `Agents/StandardAgentTests.cs` dropped: they test `Compendium.Application.AI.Agents` types that stay in the framework repo (with those same tests). The abstraction is NOT yet removed from the framework repo — that is a later, separate step. |
| `src/Compendium.Adapters.Anthropic` (+ unit tests) | `sassy-solutions/compendium-adapter-anthropic` | `823fd138b32c6c07d102159cb5a6e57597808483` | |
| `src/Compendium.Adapters.AzureOpenAI` (+ unit tests) | `sassy-solutions/compendium-adapter-azure-openai` | `b76ead96a7e7e074fa548bcc03bbb61274934a66` | |
| `src/Compendium.Adapters.Bedrock` (+ unit tests) | `sassy-solutions/compendium-adapter-bedrock` | `9b8eeff64bbcc8bc77c61fa917b0ba5f4ccf244d` | |
| `src/Compendium.Adapters.DeepSeek` (+ unit tests) | `sassy-solutions/compendium-adapter-deepseek` | `cffa6bf6805d9c56bfe0572c23cf6b75dd3d795f` | |
| `src/Compendium.Adapters.Gemini` (+ unit + integration tests) | `sassy-solutions/compendium-adapter-gemini` | `45fd4b21745f776d75700bc38e72760e7391f5df` | Integration tests are `Xunit.SkippableFact`-gated on `GEMINI_API_KEY`. |
| `src/Compendium.Adapters.HuggingFace` (+ unit tests) | `sassy-solutions/compendium-adapter-huggingface` | `4779265a3d840f49d16770888301115c8e84a457` | |
| `src/Compendium.Adapters.LiteLLM` (+ unit tests) | `sassy-solutions/compendium-adapter-litellm` | `93b7e3bb240d9e0440b9628cf35dc90b18cac153` | |
| `src/Compendium.Adapters.Mercury` (+ unit tests) | `sassy-solutions/compendium-adapter-mercury` | `ea187ef134a338d1f040e8a6d3fad55d1c036e0b` | |
| `src/Compendium.Adapters.Mistral` (+ unit + integration tests) | `sassy-solutions/compendium-adapter-mistral` | `a40a4a8d414e012412d83b3ff3d5e88ab3b465cc` | Integration tests are `SkippableFact`-gated on `MISTRAL_API_KEY`. |
| `src/Compendium.Adapters.Ollama` (+ unit + integration tests) | `sassy-solutions/compendium-adapter-ollama` | `457dffc77cee3396da8bcf50b29f9305aea212b7` | Integration tests use Testcontainers (Docker-gated). |
| `src/Compendium.Adapters.OpenAI` (+ unit + integration tests) | `sassy-solutions/compendium-adapter-openai` | `72547621b49d9de9f6cf7336f7a79ea6f62d06f4` | Integration tests are `SkippableFact`-gated on `OPENAI_API_KEY`. |
| `src/Compendium.Adapters.OpenRouter` (+ unit tests) | `sassy-solutions/compendium-adapter-openrouter` | `63add7a31aefba755d05c67656addfd804fd4307` | Only adapter using Moq in tests (kept). |
| Scaffold: `Directory.Build.props`, `global.json`, `.gitignore`, `LICENSE`, `.github/dependabot.yml`, workflow shapes | `sassy-solutions/compendium-adapter-supabase` | `1f35e9329c6c044b0ccde9ffd72c37a8e272a2d6` | Freshest release.yml (GitHub Packages first, nuget.org soft-skip, multi-nupkg assert). Nupkg assert adapted to `-ge 13`; feed owner switched to `${{ github.repository_owner }}` (SCOJH). Repo URLs in Build.props point to `SCOJH/ai`. |
| `.config/dotnet-tools.json` | `compendium-adapter-anthropic` | `823fd138…` (same as above) | reportgenerator 5.5.10 (post-Dependabot). |

## Systematic transformations

1. **Abstraction in-repo**: every adapter's (and adapter test's) `<PackageReference Include="Compendium.Abstractions.AI" />`
   was replaced by a `ProjectReference` to `src/Compendium.Abstractions.AI` (in test projects it flows
   transitively via the adapter reference). This is the atomic-PR benefit of the domain repo.
2. **Central package pins** (`Directory.Packages.props`) are the UNION of the 12 source repos' pins,
   conflicts resolved highest-wins (e.g. `Microsoft.Extensions.*` 10.0.8, `NSubstitute` 5.3.0,
   `coverlet.collector` 10.0.1, `System.Text.Json` 10.0.8). Compendium base packages pinned at
   `1.0.5-preview.1` (nuget.org; sources pinned ≤1.0.1).
3. **Samples and per-adapter docs dropped** (`samples/`, `docs/`, `CHANGELOG.md` of each source repo) to
   keep the domain repo lean — src + tests + README prioritized. Each source repo's root `README.md` was
   moved to `src/<project>/README.md` so every package keeps its own packaged README.
4. **Versioning**: single MinVer train for the whole domain, starting at `v1.1.0-preview.1` — above the
   framework's `1.0.x` so domain-repo packages win resolution.

## Not done here (later, separate steps)

- Removing `Compendium.Abstractions.AI` from the framework repo.
- Archiving the 12 single-adapter source repos.
