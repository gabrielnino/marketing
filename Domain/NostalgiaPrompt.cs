using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prompts.NostalgiaRank
{
    // ---------------------------
    // 1) JSON Models (POCOs)
    // ---------------------------

    public sealed class NostalgiaRankPrompt
    {
        [JsonPropertyName("prompt_name")]
        public string PromptName { get; init; } = "";

        [JsonPropertyName("role")]
        public List<string> Role { get; init; } = [];

        [JsonPropertyName("context")]
        public NostalgiaRankContext Context { get; init; } = new();

        [JsonPropertyName("task")]
        public NostalgiaRankTask Task { get; init; } = new();
    }

    public sealed class NostalgiaRankContext
    {
        [JsonPropertyName("audience")]
        public string Audience { get; init; } = "";

        [JsonPropertyName("language_of_output")]
        public string LanguageOfOutput { get; init; } = "";

        [JsonPropertyName("platform")]
        public string Platform { get; init; } = "";

        [JsonPropertyName("historical_rule")]
        public List<string> HistoricalRule { get; init; } = [];

        [JsonPropertyName("temporal_focus")]
        public List<string> TemporalFocus { get; init; } = [];

        [JsonPropertyName("input_description")]
        public NostalgiaRankInputDescription InputDescription { get; init; } = new();
    }

    public sealed class NostalgiaRankInputDescription
    {
        [JsonPropertyName("new_urls")]
        public string NewUrls { get; init; } = "";

        [JsonPropertyName("historical_urls")]
        public string HistoricalUrls { get; init; } = "";
    }

    public sealed class NostalgiaRankTask
    {
        [JsonPropertyName("goal")]
        public List<string> Goal { get; init; } = [];

        [JsonPropertyName("evaluation_criteria")]
        public NostalgiaRankEvaluationCriteria EvaluationCriteria { get; init; } = new();
    }

    public sealed class NostalgiaRankEvaluationCriteria
    {
        [JsonPropertyName("score_viral")]
        public string ScoreViral { get; init; } = "";
    }

    // ---------------------------
    // 2) Loader (reads JSON file)
    // ---------------------------

    public interface INostalgiaRankPromptLoader
    {
        Task<NostalgiaRankPrompt> LoadAsync(string jsonPath, CancellationToken ct = default);
    }

    public sealed class NostalgiaRankPromptLoader : INostalgiaRankPromptLoader
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public async Task<NostalgiaRankPrompt> LoadAsync(string jsonPath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
                throw new ArgumentException("JSON path is required.", nameof(jsonPath));

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("Prompt JSON file not found.", jsonPath);

            var json = await File.ReadAllTextAsync(jsonPath, ct).ConfigureAwait(false);

            var prompt = JsonSerializer.Deserialize<NostalgiaRankPrompt>(json, JsonOptions);
            if (prompt is null)
                throw new InvalidOperationException("Failed to deserialize NostalgiaRankPrompt from JSON.");

            return prompt;
        }
    }

    // ---------------------------
    // 3) Optional: placeholder replacement (if you need it later)
    //    Replaces "__NEW_URLS__" and "__HISTORICAL_URLS__" with JSON arrays.
    // ---------------------------

    public static class NostalgiaRankPromptPlaceholderReplacer
    {
        public static NostalgiaRankPrompt ReplaceUrlPlaceholders(
            NostalgiaRankPrompt prompt,
            IReadOnlyList<string> newUrls,
            IReadOnlyList<string> historicalUrls)
        {
            if (prompt is null) throw new ArgumentNullException(nameof(prompt));

            string newUrlsJson = JsonSerializer.Serialize(newUrls ?? []);
            string historicalUrlsJson = JsonSerializer.Serialize(historicalUrls ?? []);

            var input = prompt.Context.InputDescription;

            // Keep original strings, but replace tokens if present.
            var replacedInput = new NostalgiaRankInputDescription
            {
                NewUrls = (input.NewUrls ?? "").Replace("__NEW_URLS__", newUrlsJson),
                HistoricalUrls = (input.HistoricalUrls ?? "").Replace("__HISTORICAL_URLS__", historicalUrlsJson)
            };

            // Copy with updated nested object (init-only friendly).
            return new NostalgiaRankPrompt
            {
                PromptName = prompt.PromptName,
                Role = prompt.Role,
                Context = new NostalgiaRankContext
                {
                    Audience = prompt.Context.Audience,
                    LanguageOfOutput = prompt.Context.LanguageOfOutput,
                    Platform = prompt.Context.Platform,
                    HistoricalRule = prompt.Context.HistoricalRule,
                    TemporalFocus = prompt.Context.TemporalFocus,
                    InputDescription = replacedInput
                },
                Task = prompt.Task
            };
        }
    }
}
