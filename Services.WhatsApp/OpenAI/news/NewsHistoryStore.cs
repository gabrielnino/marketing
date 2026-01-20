using Services.WhatsApp.Abstractions.OpenAI.news;
using System.Text.Json;

namespace Services.WhatsApp.OpenAI.news
{
    public sealed class NewsHistoryStore(string historyPath) : INewsHistoryStore
    {
        private readonly string _historyPath = historyPath ?? throw new ArgumentNullException(nameof(historyPath));
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        

        public async Task<HashSet<string>> LoadUsedUrlsAsync(CancellationToken ct = default)
        {
            EnsureFileExists();
            await using var fs = File.OpenRead(_historyPath);
            var doc = await JsonSerializer.DeserializeAsync<HistoryDoc>(fs, JsonOpts, ct);
            return (doc?.Urls ?? [])
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .ToHashSet(StringComparer.Ordinal);
        }

        public async Task AppendUsedUrlsAsync(IEnumerable<string> urls, CancellationToken ct = default)
        {
            var toAdd = urls
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .ToList();

            if (toAdd.Count == 0) return;

            var used = await LoadUsedUrlsAsync(ct);
            var changed = false;
            var items = toAdd.Where(u => used.Add(u)).Select(u => new { });
            if(items.Any())
            {
                changed = true;
            }


            if (!changed)
            {
                return;
            }

            var doc = new HistoryDoc 
            { 
                Urls = [.. used.OrderBy(x => x, StringComparer.Ordinal)] 
            };

            await using var fs = File.Create(_historyPath);
            await JsonSerializer.SerializeAsync(fs, doc, JsonOpts, ct);
        }

        private void EnsureFileExists()
        {
            var dir = Path.GetDirectoryName(_historyPath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(_historyPath))
            {
                File.WriteAllText(_historyPath, JsonSerializer.Serialize(new HistoryDoc { Urls = [] }, JsonOpts));
            }
        }

        private sealed class HistoryDoc
        {
            public List<string> Urls { get; set; } = [];
        }
    }
}
