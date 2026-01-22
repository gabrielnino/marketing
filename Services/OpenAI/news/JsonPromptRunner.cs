// === FILE: F:\Marketing\Services\OpenAI\news\JsonPromptRunner.cs ===

using Common.StringExtensions;
using Domain;
using Domain.OpenAI;
using Microsoft.Extensions.Logging;
using Services.Abstractions.OpenAI;
using Services.Abstractions.OpenAI.news;
using Services.Abstractions.UrlValidation;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Services.OpenAI.news
{
    public class JsonPromptRunner(
        INewsHistoryStore newsHistoryStore,
        INostalgiaPromptLoader nostalgiaPromptLoader,
        IOpenAIClient openAIClient,
        IUrlAvailabilityValidatorFactory urlValidatorFactory,
        ILogger<JsonPromptRunner> logger
    ) : IJsonPromptRunner
    {
        private readonly INewsHistoryStore _newsHistoryStore = newsHistoryStore;
        private readonly INostalgiaPromptLoader _nostalgiaPromptLoader = nostalgiaPromptLoader;
        private readonly IOpenAIClient _openAIClient = openAIClient;
        private readonly IUrlAvailabilityValidatorFactory _urlValidatorFactory = urlValidatorFactory;
        private readonly ILogger<JsonPromptRunner> _logger = logger;

        // Adjust to your needs or move to IConfiguration/Options
        private const int DesiredCount = 5;
        private static readonly TimeSpan MaxOverallDuration = TimeSpan.FromMinutes(2);
        private const int DegreeOfParallelism = 8;

        public async Task<List<string>> RunStrictJsonAsync(CancellationToken ct = default)
        {
            var history = await _newsHistoryStore.LoadUrlsAsync(ct);
            var attempted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var promptNostalgia = await _nostalgiaPromptLoader.LoadPromptAsync();
            promptNostalgia.Context.ExcludedUrls = [];

            var deadline = DateTimeOffset.UtcNow + MaxOverallDuration;

            while (DateTimeOffset.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                // Build exclusions: history + attempted
                promptNostalgia.Context.ExcludedUrls.Clear();
                if (history.Count > 0) promptNostalgia.Context.ExcludedUrls.AddRange(history);
                if (attempted.Count > 0) promptNostalgia.Context.ExcludedUrls.AddRange(attempted);

                var content = ToJsonWithoutRole(promptNostalgia);
                var prompt = new Prompt
                {
                    SystemContent = promptNostalgia.Role,
                    UserContent = content
                };

                var llmRaw = await _openAIClient.GetChatCompletionAsync(prompt, ct);

                var candidates = ExtractUrlsFromLlmResponse(llmRaw.ExtractJsonContent())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (candidates.Count == 0)
                {
                    _logger.LogWarning("LLM returned no URLs. Retrying.");
                    continue;
                }

                foreach (var c in candidates) attempted.Add(c);

                // All-or-nothing validation for this batch:
                // If ANY invalid => reject whole batch, retry LLM until timeout.
                var (allValid, validUrls, results) = await ValidateBatchAllOrNothingAsync(
                    candidates,
                    desiredCount: DesiredCount,
                    degreeOfParallelism: DegreeOfParallelism,
                    ct);

                if (allValid)
                {
                    // Optionally persist into history store so the next run excludes them
                    await _newsHistoryStore.AppendUsedUrlsAsync(validUrls, ct);
                    return [.. validUrls];
                }

                //// Log first failure reason (for tuning patterns)
                var firstFail = results.FirstOrDefault(r => !r.IsValid);
                if (firstFail is not null)
                {
                    _logger.LogInformation(
                        "Batch rejected. First failure: Platform={Platform} Url={Url} Status={Status} Reason={Reason}",
                        firstFail.Platform, candidates.FirstOrDefault(u => u.Equals(u, StringComparison.OrdinalIgnoreCase)),
                        firstFail.HttpStatusCode, firstFail.FailureReason);
                }
            }

            return new List<string>(); // timeout / cancelled
        }

        private async Task<(bool AllValid, IReadOnlyList<string> ValidUrls, IReadOnlyList<UrlValidationResult> Results)>
            ValidateBatchAllOrNothingAsync(
                IReadOnlyList<string> urls,
                int desiredCount,
                int degreeOfParallelism,
                CancellationToken ct)
        {
            // Keep only up to desiredCount for strict batch semantics.
            var batch = urls.Take(desiredCount).ToList();
            if (batch.Count < desiredCount)
                return (false, Array.Empty<string>(), new[]
                {
                    new UrlValidationResult(false, UrlPlatform.Unknown, null, $"Not enough URLs. Needed={desiredCount}, Got={batch.Count}")
                });

            // Parallel validate by invoking the FACTORY per URL
            var results = new UrlValidationResult[batch.Count];
            using var sem = new SemaphoreSlim(Math.Max(1, degreeOfParallelism));

            var tasks = batch.Select(async (url, idx) =>
            {
                await sem.WaitAsync(ct);
                try
                {
                    var validator = _urlValidatorFactory.GetValidator(url);
                    results[idx] = await validator.ValidateAsync(url, ct);
                }
                catch (Exception ex)
                {
                    results[idx] = new UrlValidationResult(false, UrlPlatform.Unknown, null, ex.Message);
                }
                finally
                {
                    sem.Release();
                }
            });

            await Task.WhenAll(tasks);

            var allValid = results.All(r => r.IsValid);
            if (!allValid)
                return (false, Array.Empty<string>(), results);

            return (true, batch, results);
        }

        // ---- Parsing helpers (robust to different JSON shapes) ----

        private static List<string> ExtractUrlsFromLlmResponse(string llmRaw)
        {
            // 1) Try JSON parse
            try
            {
                using var doc = JsonDocument.Parse(llmRaw);
                var root = doc.RootElement;

                // a) root array: ["url1","url2",...]
                if (root.ValueKind == JsonValueKind.Array)
                    return root.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                        .Where(IsProbablyUrl)
                        .ToList();

                // b) { "urls": [ ... ] }
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("urls", out var urlsEl) && urlsEl.ValueKind == JsonValueKind.Array)
                    return urlsEl.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                        .Where(IsProbablyUrl)
                        .ToList();

                // c) { "items": [ { "url": "..." }, ... ] }
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
                    return itemsEl.EnumerateArray()
                        .Select(e => e.ValueKind == JsonValueKind.Object && e.TryGetProperty("url", out var u) && u.ValueKind == JsonValueKind.String ? u.GetString() : null)
                        .Where(u => !string.IsNullOrWhiteSpace(u) && IsProbablyUrl(u!))
                        .Cast<string>()
                        .ToList();
            }
            catch
            {
                // ignore; fall back to regex
            }

            // 2) Regex fallback for any text response
            return Regex.Matches(llmRaw, @"https?://[^\s""'<>]+", RegexOptions.IgnoreCase)
                .Select(m => TrimUrl(m.Value))
                .Where(IsProbablyUrl)
                .ToList();
        }

        private static bool IsProbablyUrl(string s)
            => Uri.TryCreate(s, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        private static string TrimUrl(string url)
            => url.Trim().TrimEnd('.', ',', ';', ')', ']', '}', '"', '\'');

        // ---- Existing helpers kept as-is ----

        public static string ToJsonWithoutRole(NostalgiaPrompt prompt, bool indented = true)
        {
            if (prompt is null) throw new ArgumentNullException(nameof(prompt));
            var options = new JsonSerializerOptions { WriteIndented = indented };
            JsonNode node = JsonSerializer.SerializeToNode(prompt, options)
                ?? throw new JsonException("Failed to serialize NostalgiaPrompt.");
            node.AsObject().Remove("role");
            return node.ToJsonString(options);
        }

        public static string ConvertJsonWithoutRole(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON input cannot be null or empty.", nameof(json));

            var node = JsonNode.Parse(json) ?? throw new JsonException("Invalid JSON.");
            node.AsObject().Remove("role");
            return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
