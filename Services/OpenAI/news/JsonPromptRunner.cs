// === FILE: F:\Marketing\Services\OpenAI\news\JsonPromptRunner.cs ===
// Goal: fully linear, step-by-step execution (one entity at a time)
// - No parallel tasks
// - Fail-fast all-or-nothing
// - Structured steps for debugging
// - Each step isolated into small methods
//
// MODIFICATION:
// - Instead of asking the LLM to generate URLs, this runner now calls YouTube:
//   Task<Operation<SearchResponse>> SearchVideosAsync(string query, SearchOptions options);

using Application.Result;
using Common.StringExtensions;
using Configuration.YouTube;
using Domain;
using Domain.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prompts.NostalgiaRank;
using Services.Abstractions.OpenAI;
using Services.Abstractions.OpenAI.news;
using Services.Abstractions.UrlValidation;
using Services.Abstractions.YouTube;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Services.OpenAI.news
{
    public class JsonPromptRunner(
        INewsHistoryStore newsHistoryStore,
        INostalgiaPromptLoader nostalgiaPromptLoader,
        IOpenAIClient openAIClient,
        IUrlFactory urlValidatorFactory,
        IYouTubeService youTubeService,
        IOptions<YouTubeCurationRunnerOptions> youTubeRunnerOptions,
        ILogger<JsonPromptRunner> logger
    ) : IJsonPromptRunner
    {
        private readonly INewsHistoryStore _newsHistoryStore = newsHistoryStore;
        private readonly INostalgiaPromptLoader _nostalgiaPromptLoader = nostalgiaPromptLoader;
        private readonly IOpenAIClient _openAIClient = openAIClient; // kept for compatibility (may be used later)
        private readonly IUrlFactory _urlValidatorFactory = urlValidatorFactory;
        private readonly IYouTubeService _youTubeService = youTubeService;
        private readonly YouTubeCurationRunnerOptions _yt = youTubeRunnerOptions.Value;
        private readonly ILogger<JsonPromptRunner> _logger = logger;

        private const int DesiredCount = 5;
        private static readonly TimeSpan MaxOverallDuration = TimeSpan.FromMinutes(2);

        public async Task<List<string>> RunStrictJsonAsync(CancellationToken ct = default)
        {
            var runId = Guid.NewGuid().ToString("N")[..8];
            var startedUtc = DateTimeOffset.UtcNow;

            _logger.LogInformation("[RUN {RunId}] START JsonPromptRunner. DesiredCount={DesiredCount}, Timeout={TimeoutSeconds}s, Query={Query}",
                runId, DesiredCount, (int)MaxOverallDuration.TotalSeconds, _yt.Query);

            try
            {
                // STEP 0: Pre-flight sanity checks
                Step0_Preflight(runId);

                // STEP 1: Load history
                var history = await LoadHistoryAsync(runId, ct);

                // STEP 2: Load prompt template (kept; useful for auditing / future LLM analysis)
                var promptNostalgia = await LoadPromptAsync(runId, ct);

                // STEP 3: Setup iteration memory
                var attempted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var deadline = DateTimeOffset.UtcNow + MaxOverallDuration;

                _logger.LogInformation("[RUN {RunId}] STEP 3: Init iteration state. HistoryCount={HistoryCount}, DeadlineUtc={DeadlineUtc:o}",
                    runId, history.Count, deadline);

                // STEP 4: Iterate until we succeed or timeout
                var loop = 0;
                while (DateTimeOffset.UtcNow < deadline && !ct.IsCancellationRequested)
                {
                    loop++;
                    _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: Begin. AttemptedCount={AttemptedCount}, UtcNow={UtcNow:o}",
                        runId, loop, attempted.Count, DateTimeOffset.UtcNow);

                    // STEP 4.1: (Optional) Update excluded URLs (history + attempted)
                    //UpdateExcludedUrls(runId, promptNostalgia, history, attempted);

                    // STEP 4.2: Call YouTube Search (ground truth candidates)
                    var searchOp = await CallYouTubeSearchAsync(runId, loop, _yt.Query, _yt.Search, ct);
                    if (!searchOp.IsSuccessful || searchOp.Data is null || searchOp.Data.Items.Count == 0)
                    {
                        _logger.LogWarning("[RUN {RunId}] LOOP {Loop}: STEP 4.2 YouTube search returned no items. Success={Success}, Items={Items}. Retrying.",
                            runId, loop, searchOp.IsSuccessful, searchOp.Data?.Items.Count ?? 0);
                        continue;
                    }

                    // STEP 4.3: Convert search results to candidate URLs, apply exclusions
                    var candidatesFromYoutube = ExtractCandidateUrlsFromYouTube(runId, loop, searchOp.Data, history, attempted);
                    if (candidatesFromYoutube.Count == 0)
                    {
                        _logger.LogWarning("[RUN {RunId}] LOOP {Loop}: STEP 4.3 No candidate URLs after exclusions (history/attempted). Retrying.",
                            runId, loop);
                        continue;
                    }

                    // STEP 4.4: Build Prompt (inject history + new urls)
                    var prompt = BuildPrompt(runId, loop, promptNostalgia, [.. history], candidatesFromYoutube);

                    // STEP 4.5: Call LLM (rank/filter)
                    var llmRaw = await CallLlmAsync(runId, loop, prompt, ct);

                    // STEP 4.6: Extract candidate URLs from LLM response
                    var candidatesFiltered = ExtractCandidateUrls(runId, loop, llmRaw);

                    // BUGFIX: you were checking candidates.Count (YouTube list) instead of candidatesFiltered.Count (LLM output)
                    if (candidatesFiltered.Count == 0)
                    {
                        _logger.LogWarning("[RUN {RunId}] LOOP {Loop}: STEP 4.6 LLM returned no URLs. Retrying.",
                            runId, loop);
                        AddAttempted(attempted, candidatesFromYoutube);
                        continue;
                    }

                    // STEP 4.7: Track attempted (so next loop excludes them)
                    AddAttempted(attempted, candidatesFromYoutube);
                    _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.7 Attempted updated. AttemptedCount={AttemptedCount}",
                        runId, loop, attempted.Count);

                    // STEP 4.8: Validate URLs linearly (one by one)
                    var validation = await ValidateCandidatesUntilDesiredAsync(runId, loop, candidatesFiltered, DesiredCount, ct);

                    // STEP 4.9: Persist attempted/used to history (so we don’t keep rediscovering them)
                    await AppendUsedUrlsAsync(runId, loop, candidatesFiltered, ct);

                    // STEP 4.10: If we reached desired count, return valid list
                    if (validation.Success && validation.ValidUrls.Count == DesiredCount)
                    {
                        _logger.LogInformation("[RUN {RunId}] SUCCESS. ValidCount={Count}. ElapsedMs={ElapsedMs}",
                            runId, validation.ValidUrls.Count, (DateTimeOffset.UtcNow - startedUtc).TotalMilliseconds);

                        return [.. validation.ValidUrls];
                    }

                    // STEP 4.11: Log first failure and retry loop
                    LogFirstFailure(runId, loop, validation.Results);

                    _logger.LogWarning("[RUN {RunId}] LOOP {Loop}: Not enough valid URLs. ValidCount={ValidCount}/{DesiredCount}. Retrying.",
                        runId, loop, validation.ValidUrls.Count, DesiredCount);
                }

                _logger.LogWarning("[RUN {RunId}] END: Timed out or cancelled. Cancelled={Cancelled}. ElapsedMs={ElapsedMs}",
                    runId, ct.IsCancellationRequested, (DateTimeOffset.UtcNow - startedUtc).TotalMilliseconds);

                return new List<string>();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[RUN {RunId}] CANCELLED. ElapsedMs={ElapsedMs}",
                    runId, (DateTimeOffset.UtcNow - startedUtc).TotalMilliseconds);
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] FAILED with unhandled exception. ElapsedMs={ElapsedMs}",
                    runId, (DateTimeOffset.UtcNow - startedUtc).TotalMilliseconds);
                return new List<string>();
            }
        }

        // -----------------------------
        // STEP 0: Pre-flight checks
        // -----------------------------
        private void Step0_Preflight(string runId)
        {
            _logger.LogDebug("[RUN {RunId}] STEP 0: Preflight checks...", runId);

            if (_yt is null)
                throw new InvalidOperationException("YouTubeCurationRunnerOptions is null.");

            if (string.IsNullOrWhiteSpace(_yt.Query))
                _logger.LogWarning("[RUN {RunId}] STEP 0: YouTube query is empty/blank.", runId);

            if (_yt.Search is null)
                _logger.LogWarning("[RUN {RunId}] STEP 0: YouTube search options are null.", runId);

            _logger.LogDebug("[RUN {RunId}] STEP 0: Preflight completed.", runId);
        }

        // -----------------------------
        // STEP HELPERS (LINEAR/DEBUGGABLE)
        // -----------------------------

        private List<string> ExtractCandidateUrls(string runId, int loop, string llmRaw)
        {
            _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.6 Extracting URLs from LLM response. RawLen={Len}",
                runId, loop, llmRaw?.Length ?? 0);

            var extracted = llmRaw.ExtractJsonContent();
            _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.6 JSON extracted from LLM. ExtractedLen={Len}",
                runId, loop, extracted?.Length ?? 0);

            var urls = ExtractUrlsFromLlmResponse(extracted ?? string.Empty)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.6 Extracted {Count} candidate URLs from LLM.",
                runId, loop, urls.Count);

            return urls;
        }

        private async Task<HashSet<string>> LoadHistoryAsync(string runId, CancellationToken ct)
        {
            _logger.LogInformation("[RUN {RunId}] STEP 1: Loading history URLs...", runId);

            var history = await _newsHistoryStore.LoadUrlsAsync(ct);

            _logger.LogInformation("[RUN {RunId}] STEP 1: Loaded {Count} historical URLs.", runId, history.Count);

            return history;
        }

        private async Task AppendUsedUrlsAsync(string runId, int loop, IEnumerable<string> urls, CancellationToken ct)
        {
            var list = urls?.ToList() ?? new List<string>();

            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.9 Appending used URLs to history. Count={Count}",
                runId, loop, list.Count);

            if (list.Count == 0)
            {
                _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.9 No URLs to append. Skipping.", runId, loop);
                return;
            }

            await _newsHistoryStore.AppendUsedUrlsAsync(list, ct);

            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.9 Appended {Count} URLs to history.",
                runId, loop, list.Count);
        }

        private async Task<NostalgiaRankPrompt> LoadPromptAsync(string runId, CancellationToken ct)
        {
            _logger.LogInformation("[RUN {RunId}] STEP 2: Loading NostalgiaPrompt template...", runId);

            // NOTE: ct not used because current loader signature has no ct; keep step logging anyway.
            var prompt = await _nostalgiaPromptLoader.LoadPromptAsync();

            _logger.LogInformation("[RUN {RunId}] STEP 2: Prompt loaded. RoleLines={RoleLines}.",
                runId, prompt?.Role?.Count ?? 0);

            return prompt;
        }

        //private void UpdateExcludedUrls(string runId, NostalgiaRankPrompt prompt, HashSet<string> history, HashSet<string> attempted)
        //{
        //    _logger.LogDebug("[RUN {RunId}] STEP 4.1 Updating exclusions (history + attempted)...", runId);
        //    prompt.Context.ExcludedUrls.Clear();
        //
        //    if (history.Count > 0)
        //        prompt.Context.ExcludedUrls.AddRange(history);
        //
        //    if (attempted.Count > 0)
        //        prompt.Context.ExcludedUrls.AddRange(attempted);
        //
        //    _logger.LogDebug("[RUN {RunId}] STEP 4.1 ExcludedUrls updated. Total={Count}.", runId, prompt.Context.ExcludedUrls.Count);
        //}

        private async Task<Operation<SearchResponse>> CallYouTubeSearchAsync(
            string runId,
            int loop,
            string query,
            SearchOptions options,
            CancellationToken ct)
        {
            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.2 Calling YouTube Search API. Query={Query}",
                runId, loop, query);

            var op = await _youTubeService.SearchVideosAsync(query, options);

            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.2 YouTube Search completed. Success={Success}, Items={Items}",
                runId, loop, op.IsSuccessful, op.Data?.Items.Count ?? 0);

            return op;
        }

        private List<string> ExtractCandidateUrlsFromYouTube(
            string runId,
            int loop,
            SearchResponse response,
            HashSet<string> history,
            HashSet<string> attempted)
        {
            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.3 Building candidate URLs from YouTube items...",
                runId, loop);

            var urls =
                response.Items
                    .Select(i => $"https://www.youtube.com/watch?v={i.VideoId}")
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Where(u => !history.Contains(u))
                    .Where(u => !attempted.Contains(u))
                    .ToList();

            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.3 Candidate URLs after exclusions: {Count}",
                runId, loop, urls.Count);

            return urls;
        }

        private static void AddAttempted(HashSet<string> attempted, List<string> candidates)
        {
            foreach (var c in candidates)
                attempted.Add(c);
        }

        private async Task<(bool Success, IReadOnlyList<string> ValidUrls, IReadOnlyList<UrlValidationResult> Results)>
            ValidateCandidatesUntilDesiredAsync(
                string runId,
                int loop,
                IReadOnlyList<string> urls,
                int desiredCount,
                CancellationToken ct)
        {
            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.8 Validating URLs linearly. Target={DesiredCount}, InputCount={InputCount}",
                runId, loop, desiredCount, urls.Count);

            var validUrls = new List<string>(desiredCount);
            var results = new List<UrlValidationResult>(urls.Count);

            var index = 0;
            foreach (var url in urls)
            {
                index++;
                ct.ThrowIfCancellationRequested();

                _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8 Validating URL {Index}/{Total}: {Url}",
                    runId, loop, index, urls.Count, url);

                var res = await ValidateOneUrlAsync(runId, loop, url, ct);
                results.Add(res);

                if (res.IsValid)
                {
                    validUrls.Add(url);

                    _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.8 URL valid ({Valid}/{Desired}).",
                        runId, loop, validUrls.Count, desiredCount);

                    if (validUrls.Count == desiredCount)
                    {
                        _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.8 Desired number of valid URLs reached.",
                            runId, loop);

                        return (true, validUrls, results);
                    }
                }
                else
                {
                    _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8 URL invalid. Platform={Platform} Status={Status} Reason={Reason}",
                        runId, loop, res.Platform, res.HttpStatusCode, res.FailureReason);
                }
            }

            _logger.LogWarning("[RUN {RunId}] LOOP {Loop}: STEP 4.8 Validation finished. Valid={Valid}/{Desired}.",
                runId, loop, validUrls.Count, desiredCount);

            return (false, validUrls, results);
        }

        private async Task<UrlValidationResult> ValidateOneUrlAsync(string runId, int loop, string url, CancellationToken ct)
        {
            _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 ValidateOneUrl start. Url={Url}",
                runId, loop, url);

            // Explicit URL format check (debug-friendly)
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Invalid absolute URL format. Url={Url}",
                    runId, loop, url);

                return new UrlValidationResult(false, UrlPlatform.Unknown, null, "Invalid absolute URL.");
            }

            try
            {
                _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Resolving validator from factory...", runId, loop);

                // Factory resolves correct validator based on URL/platform
                var validator = _urlValidatorFactory.GetValidator(url);

                _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Validator resolved: {ValidatorType}",
                    runId, loop, validator.GetType().Name);

                // Single validation call
                var result = await validator.ValidateAsync(url, ct);

                _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Validation done. IsValid={IsValid} Platform={Platform} Status={Status} Reason={Reason}",
                    runId, loop, result.IsValid, result.Platform, result.HttpStatusCode, result.FailureReason);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[RUN {RunId}] LOOP {Loop}: STEP 4.8.1 Validation exception for Url={Url}",
                    runId, loop, url);

                return new UrlValidationResult(false, UrlPlatform.Unknown, null, ex.Message);
            }
        }

        private void LogFirstFailure(string runId, int loop, IReadOnlyList<UrlValidationResult> results)
        {
            var firstFail = results.FirstOrDefault(r => !r.IsValid);
            if (firstFail is null)
            {
                _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.11 No failures to report (all valid).",
                    runId, loop);
                return;
            }

            _logger.LogInformation(
                "[RUN {RunId}] LOOP {Loop}: STEP 4.11 Batch rejected. First failure: Platform={Platform} Status={Status} Reason={Reason}",
                runId,
                loop,
                firstFail.Platform,
                firstFail.HttpStatusCode,
                firstFail.FailureReason);
        }

        // -----------------------------
        // URL extraction (kept for compatibility)
        // -----------------------------
        private static List<string> ExtractUrlsFromLlmResponse(string llmRaw)
        {
            try
            {
                using var doc = JsonDocument.Parse(llmRaw);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                    return [.. root.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                        .Where(IsProbablyUrl)];

                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("urls", out var urlsEl) &&
                    urlsEl.ValueKind == JsonValueKind.Array)
                {
                    return [.. urlsEl.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                        .Where(IsProbablyUrl)];
                }

                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("items", out var itemsEl) &&
                    itemsEl.ValueKind == JsonValueKind.Array)
                {
                    return [.. itemsEl.EnumerateArray()
                        .Select(e => e.ValueKind == JsonValueKind.Object &&
                                    e.TryGetProperty("url", out var u) &&
                                    u.ValueKind == JsonValueKind.String
                                        ? u.GetString()
                                        : null)
                        .Where(u => !string.IsNullOrWhiteSpace(u) && IsProbablyUrl(u!))
                        .Cast<string>()];
                }
            }
            catch
            {
                // ignore; fall back to regex
            }

            return [.. Regex.Matches(llmRaw, @"https?://[^\s""'<>]+", RegexOptions.IgnoreCase)
                .Select(m => TrimUrl(m.Value))
                .Where(IsProbablyUrl)];
        }

        private static bool IsProbablyUrl(string s)
            => Uri.TryCreate(s, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        private static string TrimUrl(string url)
            => url.Trim().TrimEnd('.', ',', ';', ')', ']', '}', '"', '\'');

        // -----------------------------
        // JSON helpers
        // -----------------------------
        public static string ToJsonWithoutRole(NostalgiaRankPrompt prompt, bool indented = true)
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

        private Prompt BuildPrompt(
            string runId,
            int loop,
            NostalgiaRankPrompt promptNostalgia,
            IReadOnlyList<string> historicalUrls,
            IReadOnlyList<string> newUrls)
        {
            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.4 Building LLM prompt (System + User JSON without role)...",
                runId, loop);

            ArgumentNullException.ThrowIfNull(promptNostalgia);

            historicalUrls ??= [];
            newUrls ??= [];

            _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.4 Injecting URLs into JSON. HistoricalCount={HCount}, NewCount={NCount}",
                runId, loop, historicalUrls.Count, newUrls.Count);

            // Build the JSON body for the user message (without role/system content).
            var content = ToJsonWithoutRole(promptNostalgia);

            // Replace JSON-safe tags with real JSON arrays.
            content = ReplaceJsonTagWithArray(content, "__HISTORICAL_URLS__", historicalUrls);
            content = ReplaceJsonTagWithArray(content, "__NEW_URLS__", newUrls);

            var systemContent = string.Join(" ", promptNostalgia.Role.Where(r => !string.IsNullOrWhiteSpace(r)));

            _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.4 Prompt built. SystemLen={SysLen}, UserLen={UserLen}",
                runId, loop, systemContent.Length, content.Length);

            return new Prompt
            {
                SystemContent = systemContent,
                UserContent = content
            };
        }

        private static string ReplaceJsonTagWithArray(
            string json,
            string tag,
            IReadOnlyList<string> urls)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            urls ??= [];

            // Serialize the array itself (e.g. ["https://...","https://..."])
            var arrayJson = JsonSerializer.Serialize(urls);

            // The tag is inside JSON as a string value, so replace the quoted token:
            // "new_urls": "__NEW_URLS__"
            // becomes
            // "new_urls": ["...","..."]
            var quotedTag = JsonSerializer.Serialize(tag); // yields "\"__NEW_URLS__\""

            return json.Replace(quotedTag, arrayJson, StringComparison.Ordinal);
        }

        private async Task<string> CallLlmAsync(string runId, int loop, Prompt prompt, CancellationToken ct)
        {
            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.5 Calling LLM...",
                runId, loop);

            _logger.LogDebug("[RUN {RunId}] LOOP {Loop}: STEP 4.5 Prompt sizes. SystemLen={SysLen}, UserLen={UserLen}",
                runId, loop, prompt.SystemContent?.Length ?? 0, prompt.UserContent?.Length ?? 0);

            var raw = await _openAIClient.GetChatCompletionAsync(prompt, ct);

            _logger.LogInformation("[RUN {RunId}] LOOP {Loop}: STEP 4.5 LLM response received. Length={Len}",
                runId, loop, raw?.Length ?? 0);

            return raw ?? string.Empty;
        }
    }
}
