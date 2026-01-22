using Domain.WhatsApp;
using Services.Abstractions.OpenAI.news;
using System.Text.Json;

namespace Services.WhatsApp.OpenAI.news
{
    public class NostalgiaPromptLoader: INostalgiaPromptLoader
    {
        private static readonly JsonSerializerOptions Opts = new() { PropertyNameCaseInsensitive = true };
        private const string DefaultNameFilePrompt = "NostalgiaURLSelector.json";

        public async Task<NostalgiaPrompt> LoadPromptAsync()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "OpenAI", "news", DefaultNameFilePrompt);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Prompt file not found: {path}");

            var json = await File.ReadAllTextAsync(path);
            var cfg = JsonSerializer.Deserialize<NostalgiaPrompt>(json, Opts);

            return cfg ?? throw new InvalidOperationException("Prompt JSON is invalid or empty.");
        }
    }
}
