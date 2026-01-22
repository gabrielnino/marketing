using Services.Abstractions.OpenAI.news;
using System.Text.Json;

namespace Services.OpenAI.news
{
    public sealed class NewsHistoryStore : INewsHistoryStore
    {
        private const string DefaultNameFileNewsHistoryStore = "NewsHistoryStore.json";
        private readonly string _historyPath = Path.Combine(AppContext.BaseDirectory, DefaultNameFileNewsHistoryStore);

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public async Task<HashSet<string>> LoadUrlsAsync(CancellationToken ct = default)
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

            var used = await LoadUrlsAsync(ct);
            var changed = toAdd.Any(u => used.Add(u));
            if (!changed) return;

            var doc = new HistoryDoc
            {
                Urls = [.. used.OrderBy(x => x, StringComparer.Ordinal)]
            };

            await WriteJsonAtomicallyAsync(_historyPath, doc, ct);
        }

        private static async Task WriteJsonAtomicallyAsync(string path, HistoryDoc doc, CancellationToken ct)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            // Same directory => rename/move is atomic on same volume.
            var tempPath = Path.Combine(dir ?? ".", $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

            try
            {
                // 1) Write to temp file
                await using (var fs = new FileStream(
                    tempPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 64 * 1024,
                    options: FileOptions.Asynchronous | FileOptions.WriteThrough))
                {
                    await JsonSerializer.SerializeAsync(fs, doc, JsonOpts, ct);
                    await fs.FlushAsync(ct);
                }

                // 2) Atomic swap
                if (File.Exists(path))
                {
                    // File.Replace is atomic on Windows and preserves metadata better than delete+move.
                    var backupPath = path + ".bak";
                    File.Replace(tempPath, path, backupPath, ignoreMetadataErrors: true);
                    TryDelete(backupPath); // optional: remove backup after successful replace
                }
                else
                {
                    // Atomic rename within same directory
                    File.Move(tempPath, path);
                }
            }
            finally
            {
                // If anything failed before swap, clean temp file
                TryDelete(tempPath);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // Intentionally ignore cleanup failures
            }
        }

        private void EnsureFileExists()
        {
            var dir = Path.GetDirectoryName(_historyPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

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
