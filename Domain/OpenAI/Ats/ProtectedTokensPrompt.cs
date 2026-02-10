using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.OpenAI.Ats
{
    // ---------------------------
    // 1) JSON Models (POCOs)
    // ---------------------------

    public sealed class ProtectedTokensPrompt
    {
        [JsonPropertyName("prompt_name")]
        public string PromptName { get; init; } = "";

        [JsonPropertyName("role")]
        public List<string> Role { get; init; } = [];

        [JsonPropertyName("context")]
        public ProtectedTokensContext Context { get; init; } = new();

        [JsonPropertyName("task")]
        public ProtectedTokensTask Task { get; init; } = new();
    }

    public sealed class ProtectedTokensContext
    {
        [JsonPropertyName("task_description")]
        public string TaskDescription { get; init; } = "";

        [JsonPropertyName("input_description")]
        public ProtectedTokensInputDescription InputDescription { get; init; } = new();
    }

    public sealed class ProtectedTokensInputDescription
    {
        [JsonPropertyName("job_description")]
        public string JobDescription { get; init; } = "";
    }

    public sealed class ProtectedTokensTask
    {
        [JsonPropertyName("goal")]
        public List<string> Goal { get; init; } = [];
    }

    // ---------------------------
    // 2) Loader (reads JSON file)
    // ---------------------------

    public interface IProtectedTokensPromptLoader
    {
        Task<ProtectedTokensPrompt> LoadAsync(string jsonPath, CancellationToken ct = default);
    }

    public sealed class ProtectedTokensPromptLoader : IProtectedTokensPromptLoader
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public async Task<ProtectedTokensPrompt> LoadAsync(string jsonPath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
                throw new ArgumentException("JSON path is required.", nameof(jsonPath));

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("Prompt JSON file not found.", jsonPath);

            var json = await File.ReadAllTextAsync(jsonPath, ct).ConfigureAwait(false);

            var prompt = JsonSerializer.Deserialize<ProtectedTokensPrompt>(json, JsonOptions);
            if (prompt is null)
                throw new InvalidOperationException("Failed to deserialize ProtectedTokensPrompt from JSON.");

            return prompt;
        }
    }
}
