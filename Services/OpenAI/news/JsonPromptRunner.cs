using Domain;
using Domain.OpenAI;
using Services.Abstractions.OpenAI;
using Services.Abstractions.OpenAI.news;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace Services.OpenAI.news
{
    public class JsonPromptRunner(
        INewsHistoryStore newsHistoryStore,
        INostalgiaPromptLoader nostalgiaPromptLoader,
        IOpenAIClient openAIClient
         ) : IJsonPromptRunner
    {
        private readonly INewsHistoryStore _newsHistoryStore = newsHistoryStore;
        private readonly INostalgiaPromptLoader _nostalgiaPromptLoader = nostalgiaPromptLoader;
        private readonly IOpenAIClient _openAIClient = openAIClient;

        public async Task<List<string>> RunStrictJsonAsync(CancellationToken ct = default)
        {
            var urls = await _newsHistoryStore.LoadUrlsAsync(ct);
            var promptNoslgia = await _nostalgiaPromptLoader.LoadPromptAsync();
            promptNoslgia.Context.ExcludedUrls = [];
            if (urls.Any())
            {
                promptNoslgia.Context.ExcludedUrls.AddRange(urls);
            }
            var content = ToJsonWithoutRole(promptNoslgia);
            var prompt = new Prompt
            {
                SystemContent = promptNoslgia.Role,
                UserContent = content
            };
            var result = await _openAIClient.GetChatCompletionAsync(prompt, ct);
            return ["one", "two", "three"];
        }

        public static string ToJsonWithoutRole(NostalgiaPrompt prompt, bool indented = true)
        {
            if (prompt is null) throw new ArgumentNullException(nameof(prompt));

            var options = new JsonSerializerOptions { WriteIndented = indented };

            // Serialize the class normally, then remove "role" from the JSON.
            JsonNode node = JsonSerializer.SerializeToNode(prompt, options)
                ?? throw new JsonException("Failed to serialize NostalgiaPrompt.");

            node.AsObject().Remove("role");

            return node.ToJsonString(options);
        }

        public static string ConvertJsonWithoutRole(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON input cannot be null or empty.", nameof(json));

            var node = JsonNode.Parse(json)
                       ?? throw new JsonException("Invalid JSON.");

            // Remove "role" at root level if present
            node.AsObject().Remove("role");

            return node.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}
