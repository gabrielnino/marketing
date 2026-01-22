// === FILE: F:\Marketing\Services\OpenAI\news\JsonPromptRunner.cs ===
// Goal: fully linear, step-by-step execution (one entity at a time)
// - No parallel tasks
// - Fail-fast all-or-nothing
// - Structured steps for debugging
// - Each step isolated into small methods

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

        private const int DesiredCount = 5;
        private static readonly TimeSpan MaxOverallDuration = TimeSpan.FromMinutes(2);

        public async Task<List<string>> RunStrictJsonAsync(CancellationToken ct = default)
        {
            // STEP 1: Load history
            var history = await LoadHistoryAsync(ct);

            // STEP 2: Load prompt template
            var promptNostalgia = await LoadPromptAsync(ct);

            // STEP 3: Setup iteration memory
            var attempted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var deadline = DateTimeOffset.UtcNow + MaxOverallDuration;

            // STEP 4: Iterate until we succeed or timeout
            while (DateTimeOffset.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                // STEP 4.1: Update excluded URLs
                UpdateExcludedUrls(promptNostalgia, history, attempted);

                // STEP 4.2: Build request prompt
                var prompt = BuildPrompt(promptNostalgia);

                // STEP 4.3: Call LLM
                var llmRaw = await CallLlmAsync(prompt, ct);

                // STEP 4.4: Extract candidate URLs from response
                var candidates = ExtractCandidateUrls(llmRaw);

                if (candidates.Count == 0)
                {
                    _logger.LogWarning("LLM returned no URLs. Retrying.");
                    continue;
                }

                // STEP 4.5: Track attempted (so next LLM call excludes them)
                AddAttempted(attempted, candidates);

                // STEP 4.6: Validate URLs linearly (one by one)
                var validation = await ValidateCandidatesUntilDesiredAsync(candidates, DesiredCount, ct);

                var results = validation.Results;

                await AppendUsedUrlsAsync(candidates, ct);
                //if (validation.AllValid)
                //{
                //    // STEP 4.7: Persist results
                //    await PersistValidUrlsAsync(validation.ValidUrls, ct);

                //    // STEP 4.8: Return valid list
                //    return [.. validation.ValidUrls];
                //}

                // STEP 4.9: Log first failure and retry loop
                LogFirstFailure(validation.Results);
            }

            return new List<string>(); // timeout / cancelled
        }

        // -----------------------------
        // STEP HELPERS (LINEAR/DEBUGGABLE)
        // -----------------------------

        private async Task<HashSet<string>> LoadHistoryAsync(CancellationToken ct)
        {
            _logger.LogDebug("STEP 1: Loading history URLs...");
            var history = await _newsHistoryStore.LoadUrlsAsync(ct);
            _logger.LogDebug("STEP 1: Loaded {Count} historical URLs.", history.Count);
            return history;
        }

        private async Task AppendUsedUrlsAsync(IEnumerable<string> urls, CancellationToken ct)
        {
            _logger.LogDebug("STEP 1: Loading history URLs...");
            await _newsHistoryStore.AppendUsedUrlsAsync(urls, ct);
            _logger.LogDebug("STEP 1: Loaded {Count} historical URLs.", urls.Count());
        }

        private async Task<NostalgiaPrompt> LoadPromptAsync(CancellationToken ct)
        {
            _logger.LogDebug("STEP 2: Loading NostalgiaPrompt template...");
            var prompt = await _nostalgiaPromptLoader.LoadPromptAsync();
            prompt.Context.ExcludedUrls = [];
            _logger.LogDebug("STEP 2: Prompt loaded. ExcludedUrls initialized.");
            return prompt;
        }

        private void UpdateExcludedUrls(NostalgiaPrompt prompt, HashSet<string> history, HashSet<string> attempted)
        {
            _logger.LogDebug("STEP 4.1: Updating exclusions (history + attempted)...");
            prompt.Context.ExcludedUrls.Clear();

            if (history.Count > 0)
                prompt.Context.ExcludedUrls.AddRange(history);

            if (attempted.Count > 0)
                prompt.Context.ExcludedUrls.AddRange(attempted);

            _logger.LogDebug("STEP 4.1: ExcludedUrls updated. Total={Count}.", prompt.Context.ExcludedUrls.Count);
        }

        private Prompt BuildPrompt(NostalgiaPrompt promptNostalgia)
        {
            _logger.LogDebug("STEP 4.2: Building LLM prompt (System + User JSON without role)...");
            var content = ToJsonWithoutRole(promptNostalgia);
            return new Prompt
            {
                SystemContent = promptNostalgia.Role,
                UserContent = content
            };
        }

        private async Task<string> CallLlmAsync(Prompt prompt, CancellationToken ct)
        {
            _logger.LogDebug("STEP 4.3: Calling LLM...");
            var raw = await _openAIClient.GetChatCompletionAsync(prompt, ct);
            _logger.LogDebug("STEP 4.3: LLM response received. Length={Len}", raw?.Length ?? 0);
            return raw ?? string.Empty;
        }

        private List<string> ExtractCandidateUrls(string llmRaw)
        {
            _logger.LogDebug("STEP 4.4: Extracting URLs from LLM response...");

            var extracted = llmRaw.ExtractJsonContent();
            var urls = ExtractUrlsFromLlmResponse(extracted)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            _logger.LogDebug("STEP 4.4: Extracted {Count} candidate URLs.", urls.Count);
            return urls;
        }

        private static void AddAttempted(HashSet<string> attempted, List<string> candidates)
        {
            foreach (var c in candidates)
                attempted.Add(c);
        }


        private async Task<(bool Success, IReadOnlyList<string> ValidUrls, IReadOnlyList<UrlValidationResult> Results)>
     ValidateCandidatesUntilDesiredAsync(
         IReadOnlyList<string> urls,
         int desiredCount,
         CancellationToken ct)
        {
            _logger.LogDebug("Validating URLs linearly until {Desired} valid ones are found.", desiredCount);

            var validUrls = new List<string>(desiredCount);
            var results = new List<UrlValidationResult>(urls.Count);

            foreach (var url in urls)
            {
                ct.ThrowIfCancellationRequested();

                _logger.LogDebug("Validating URL: {Url}", url);

                var res = await ValidateOneUrlAsync(url, ct);
                results.Add(res);

                if (res.IsValid)
                {
                    validUrls.Add(url);

                    _logger.LogDebug("URL valid ({Count}/{Desired})", validUrls.Count, desiredCount);

                    if (validUrls.Count == desiredCount)
                    {
                        _logger.LogDebug("Desired number of valid URLs reached.");
                        return (true, validUrls, results);
                    }
                }
            }

            _logger.LogDebug(
                "Validation finished. Only {Count}/{Desired} valid URLs found.",
                validUrls.Count, desiredCount);

            return (false, validUrls, results);
        }


        private static List<string> PrepareStrictBatch(IReadOnlyList<string> urls, int desiredCount)
        {
            return urls
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(desiredCount)
                .ToList();
        }

        private async Task<UrlValidationResult> ValidateOneUrlAsync(string url, CancellationToken ct)
        {
            // Explicit URL format check (debug-friendly)
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                return new UrlValidationResult(false, UrlPlatform.Unknown, null, "Invalid absolute URL.");

            try
            {
                // FACTORY INVOCATION (linear)
                var validator = _urlValidatorFactory.GetValidator(url);

                // Single validation call
                return await validator.ValidateAsync(url, ct);
            }
            catch (Exception ex)
            {
                return new UrlValidationResult(false, UrlPlatform.Unknown, null, ex.Message);
            }
        }

        private async Task PersistValidUrlsAsync(IReadOnlyList<string> validUrls, CancellationToken ct)
        {
            _logger.LogDebug("STEP 4.7: Persisting {Count} valid URLs into history store...", validUrls.Count);
            await _newsHistoryStore.AppendUsedUrlsAsync(validUrls, ct);
            _logger.LogDebug("STEP 4.7: Persist complete.");
        }

        private void LogFirstFailure(IReadOnlyList<UrlValidationResult> results)
        {
            var firstFail = results.FirstOrDefault(r => !r.IsValid);
            if (firstFail is null)
                return;

            _logger.LogInformation(
                "Batch rejected. First failure: Platform={Platform} Status={Status} Reason={Reason}",
                firstFail.Platform,
                firstFail.HttpStatusCode,
                firstFail.FailureReason);
        }

        // -----------------------------
        // URL extraction (unchanged)
        // -----------------------------

        private static List<string> ExtractUrlsFromLlmResponse(string llmRaw)
        {
            try
            {
                using var doc = JsonDocument.Parse(llmRaw);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                    return root.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                        .Where(IsProbablyUrl)
                        .ToList();

                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("urls", out var urlsEl) &&
                    urlsEl.ValueKind == JsonValueKind.Array)
                {
                    return urlsEl.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                        .Where(IsProbablyUrl)
                        .ToList();
                }

                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("items", out var itemsEl) &&
                    itemsEl.ValueKind == JsonValueKind.Array)
                {
                    return itemsEl.EnumerateArray()
                        .Select(e => e.ValueKind == JsonValueKind.Object &&
                                    e.TryGetProperty("url", out var u) &&
                                    u.ValueKind == JsonValueKind.String
                                        ? u.GetString()
                                        : null)
                        .Where(u => !string.IsNullOrWhiteSpace(u) && IsProbablyUrl(u!))
                        .Cast<string>()
                        .ToList();
                }
            }
            catch
            {
                // ignore; fall back to regex
            }

            return Regex.Matches(llmRaw, @"https?://[^\s""'<>]+", RegexOptions.IgnoreCase)
                .Select(m => TrimUrl(m.Value))
                .Where(IsProbablyUrl)
                .ToList();
        }

        private static bool IsProbablyUrl(string s)
            => Uri.TryCreate(s, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        private static string TrimUrl(string url)
            => url.Trim().TrimEnd('.', ',', ';', ')', ']', '}', '"', '\'');

        // -----------------------------
        // JSON helpers (unchanged)
        // -----------------------------

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
