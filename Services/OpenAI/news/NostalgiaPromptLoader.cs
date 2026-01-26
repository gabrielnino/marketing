using Prompts.NostalgiaRank;
using Services.Abstractions.OpenAI.news;
using System.Text.Json;

namespace Services.OpenAI.news
{
    public class NostalgiaPromptLoader: INostalgiaPromptLoader
    {
        private static readonly JsonSerializerOptions Opts = new() { PropertyNameCaseInsensitive = true };
        private const string DefaultNameFilePrompt = "NostalgiaRank.json";

        public async Task<NostalgiaRankPrompt> LoadPromptAsync()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "OpenAI", "news", DefaultNameFilePrompt);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Prompt file not found: {path}");

            var json = await File.ReadAllTextAsync(path);
            var cfg = JsonSerializer.Deserialize<NostalgiaRankPrompt>(json, Opts);

            return cfg ?? throw new InvalidOperationException("Prompt JSON is invalid or empty.");
        }
    }
}
