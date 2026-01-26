using Application.PixVerse;
using Application.Result;
using Configuration.PixVerse;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.PixVerse;

public sealed class PixVerseService(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<PixVerseService> logger
) : IPixVerseService
{
    // -----------------------------
    // PixVerse API paths (per screenshots)
    // Base URL: https://app-api.pixverse.ai
    // POST: /openapi/v2/video/text/generate
    // Headers: API-KEY, Ai-trace-id
    // -----------------------------
    private const string BalancePath = "/openapi/v2/account/balance";               // not shown in screenshots; keep as best-guess
    private const string TextToVideoPath = "/openapi/v2/video/text/generate";
    private const string StatusPath = "/openapi/v2/video/status/";         // best-guess
    private const string ResultPath = "/openapi/v2/video/result/";         // best-guess

    private const string ApiKeyHeader = "API-KEY";
    private const string TraceIdHeader = "Ai-trace-id";

    private readonly HttpClient _http = httpClient;
    private readonly PixVerseOptions _opt = options.Value;
    private readonly IErrorHandler _error = errorHandler;
    private readonly ILogger<PixVerseService> _logger = logger;

    private static readonly JsonSerializerOptions JsonOpts =
        new(JsonSerializerDefaults.Web);

    // -------------------------------------------------
    // Account / Billing
    // -------------------------------------------------
    public async Task<Operation<PixVerseBalance>> CheckBalanceAsync(CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START PixVerse.CheckBalance", runId);

        try
        {
            if (!TryValidateConfig(out var configError))
                return _error.Fail<PixVerseBalance>(null, configError);

            var endpoint = BuildEndpoint(BalancePath);

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            ApplyAuth(req);

            using var timeoutCts = _opt.HttpTimeout > TimeSpan.Zero
                ? new CancellationTokenSource(_opt.HttpTimeout)
                : new CancellationTokenSource();

            using var linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            using var res = await _http.SendAsync(req, linkedCts.Token);

            if (!res.IsSuccessStatusCode)
                return _error.Fail<PixVerseBalance>(
                    null,
                    $"PixVerse balance failed. HTTP {(int)res.StatusCode}");

            var json = await res.Content.ReadAsStringAsync(linkedCts.Token);

            // Some PixVerse endpoints may also return envelope; we try envelope first, then raw model.
            var env = TryDeserialize<PixVerseApiEnvelope<PixVerseBalance>>(json);
            if (env is not null)
            {
                if (env.ErrCode != 0)
                    return _error.Fail<PixVerseBalance>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

                if (env.Resp is null)
                    return _error.Fail<PixVerseBalance>(null, "Invalid balance payload (Resp null).");

                return Operation<PixVerseBalance>.Success(env.Resp, env.ErrMsg);
            }

            var balance = JsonSerializer.Deserialize<PixVerseBalance>(json, JsonOpts);
            if (balance is null)
                return _error.Fail<PixVerseBalance>(null, "Invalid balance payload");

            return Operation<PixVerseBalance>.Success(balance);
        }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "[RUN {RunId}] TIMEOUT CheckBalance after {Timeout}",
                runId,
                _opt.HttpTimeout);

            return _error.Fail<PixVerseBalance>(
                ex,
                $"Balance check timed out after {_opt.HttpTimeout}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED CheckBalance", runId);
            return _error.Fail<PixVerseBalance>(ex, "Balance check failed");
        }
    }

    // -------------------------------------------------
    // Text-to-Video
    // -------------------------------------------------
    public async Task<Operation<PixVerseJobSubmitted>> SubmitTextToVideoAsync(
        PixVerseTextToVideoRequest request,
        CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START SubmitTextToVideo", runId);

        try
        {
            request.Validate(); // current model validates Model/Prompt/Duration/Quality/Seed :contentReference[oaicite:2]{index=2}

            if (!TryValidateConfig(out var configError))
                return _error.Fail<PixVerseJobSubmitted>(null, configError);

            var endpoint = BuildEndpoint(TextToVideoPath);
            var payload = JsonSerializer.Serialize(request, JsonOpts);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            ApplyAuth(req);

            using var res = await _http.SendAsync(req, ct);

            if (!res.IsSuccessStatusCode)
                return _error.Fail<PixVerseJobSubmitted>(
                    null,
                    $"Submit failed. HTTP {(int)res.StatusCode}");

            var json = await res.Content.ReadAsStringAsync(ct);

            // Per screenshot response:
            // { "ErrCode":0, "ErrMsg":"success", "Resp": { "video_id":"...", "credits":45 } }
            var env = JsonSerializer.Deserialize<PixVerseApiEnvelope<PixVerseSubmitResp>>(json, JsonOpts);

            if (env is null)
                return _error.Fail<PixVerseJobSubmitted>(null, "Invalid submit response (null).");

            if (env.ErrCode != 0)
                return _error.Fail<PixVerseJobSubmitted>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            if (env.Resp is null || env.Resp.VideoId == 0)
                return _error.Fail<PixVerseJobSubmitted>(null, "Invalid submit response (missing Resp.video_id).");

            var submitted = new PixVerseJobSubmitted
            {
                JobId = env.Resp.VideoId,
                Message = env.ErrMsg
            };

            return Operation<PixVerseJobSubmitted>.Success(submitted, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitTextToVideo", runId);
            return _error.Fail<PixVerseJobSubmitted>(ex, "Submit failed");
        }
    }

    public async Task<Operation<PixVerseGenerationStatus>> GetGenerationStatusAsync(
        string jobId,
        CancellationToken ct = default)
    {
        if (!TryValidateConfig(out var configError))
            return _error.Fail<PixVerseGenerationStatus>(null, configError);

        var endpoint = BuildEndpoint(StatusPath + Uri.EscapeDataString(jobId));

        using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
            return _error.Fail<PixVerseGenerationStatus>(
                null,
                $"Status failed. HTTP {(int)res.StatusCode}");

        var json = await res.Content.ReadAsStringAsync(ct);

        // Best-effort: try envelope first, then raw model.
        var env = TryDeserialize<PixVerseApiEnvelope<PixVerseGenerationStatus>>(json);
        if (env is not null)
        {
            if (env.ErrCode != 0)
                return _error.Fail<PixVerseGenerationStatus>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            return env.Resp is null
                ? _error.Fail<PixVerseGenerationStatus>(null, "Invalid status payload (Resp null).")
                : Operation<PixVerseGenerationStatus>.Success(env.Resp, env.ErrMsg);
        }

        var status = JsonSerializer.Deserialize<PixVerseGenerationStatus>(json, JsonOpts);

        return status is null
            ? _error.Fail<PixVerseGenerationStatus>(null, "Invalid status payload")
            : Operation<PixVerseGenerationStatus>.Success(status);
    }

    public async Task<Operation<PixVerseGenerationResult>> GetGenerationResultAsync(
        string jobId,
        CancellationToken ct = default)
    {
        if (!TryValidateConfig(out var configError))
            return _error.Fail<PixVerseGenerationResult>(null, configError);

        var endpoint = BuildEndpoint(ResultPath + Uri.EscapeDataString(jobId));

        using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
            return _error.Fail<PixVerseGenerationResult>(
                null,
                $"Result failed. HTTP {(int)res.StatusCode}");

        var json = await res.Content.ReadAsStringAsync(ct);

        // Best-effort: try envelope first, then raw model.
        var env = TryDeserialize<PixVerseApiEnvelope<PixVerseGenerationResult>>(json);
        if (env is not null)
        {
            if (env.ErrCode != 0)
                return _error.Fail<PixVerseGenerationResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            return env.Resp is null
                ? _error.Fail<PixVerseGenerationResult>(null, "Invalid result payload (Resp null).")
                : Operation<PixVerseGenerationResult>.Success(env.Resp, env.ErrMsg);
        }

        var result = JsonSerializer.Deserialize<PixVerseGenerationResult>(json, JsonOpts);

        return result is null
            ? _error.Fail<PixVerseGenerationResult>(null, "Invalid result payload")
            : Operation<PixVerseGenerationResult>.Success(result);
    }

    public async Task<Operation<PixVerseGenerationResult>> WaitForCompletionAsync(
        string jobId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return _error.Business<PixVerseGenerationResult>("jobId cannot be null or empty.");

        for (var i = 0; i < _opt.MaxPollingAttempts; i++)
        {
            ct.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "STEP PV-4.1 - Poll {Poll}/{Max}. Getting generation status. JobId={JobId}",
                i + 1, _opt.MaxPollingAttempts, jobId);

            var st = await GetGenerationStatusAsync(jobId, ct);

            if (!st.IsSuccessful)
            {
                _logger.LogWarning(
                    "STEP PV-4.1 - Status call failed. JobId={JobId}. Type={Type}. Message={Message}",
                    jobId, st.Type, st.Message);

                return st.ConvertTo<PixVerseGenerationResult>();
            }

            if (st.Data is null)
            {
                _logger.LogWarning(
                    "STEP PV-4.1 - Status payload was null (unexpected). JobId={JobId}",
                    jobId);

                return _error.Fail<PixVerseGenerationResult>(null, "Invalid status payload (null).");
            }

            _logger.LogInformation(
                "STEP PV-4.2 - Status received. JobId={JobId}. State={State}. Progress={Progress}",
                jobId, st.Data.State, st.Data.ProgressPercent);

            if (st.Data.IsTerminal)
            {
                _logger.LogInformation(
                    "STEP PV-4.3 - Job reached terminal state. JobId={JobId}. State={State}. Fetching result...",
                    jobId, st.Data.State);

                if (st.Data.State == PixVerseJobState.Succeeded)
                    return await GetGenerationResultAsync(jobId, ct);

                var msg = $"Job ended with terminal state: {st.Data.State}.";
                return Operation<PixVerseGenerationResult>.Success(new PixVerseGenerationResult
                {
                    JobId = jobId,
                    State = st.Data.State,
                    ErrorCode = st.Data.ErrorCode,
                    ErrorMessage = st.Data.ErrorMessage ?? msg
                }, msg);
            }

            _logger.LogInformation(
                "STEP PV-4.4 - Job not terminal yet. Waiting {DelayMs}ms before next poll. JobId={JobId}",
                (int)_opt.PollingInterval.TotalMilliseconds, jobId);

            await Task.Delay(_opt.PollingInterval, ct);
        }

        _logger.LogWarning(
            "STEP PV-4.5 - Polling timed out after {Max} attempts. JobId={JobId}",
            _opt.MaxPollingAttempts, jobId);

        return _error.Fail<PixVerseGenerationResult>(null, "Polling timed out.");
    }

    // -------------------------------------------------
    // Helpers
    // -------------------------------------------------
    private static string NewRunId() => Guid.NewGuid().ToString("N")[..8];

    private bool TryValidateConfig(out string error)
    {
        if (string.IsNullOrWhiteSpace(_opt.BaseUrl))
        {
            error = "PixVerse BaseUrl is not configured.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
        {
            error = "PixVerse ApiKey is not configured.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private Uri BuildEndpoint(string pathOrPathWithId)
    {
        //TextToVideoPath
        var baseUri = new Uri(_opt.BaseUrl.TrimEnd('/') + "/");
        return new Uri(baseUri, pathOrPathWithId.TrimStart('/'));
    }

    private void ApplyAuth(HttpRequestMessage req)
    {
        // Matches screenshots: API-KEY header, NOT Bearer Authorization
        req.Headers.Remove(ApiKeyHeader);
        req.Headers.Add(ApiKeyHeader, _opt.ApiKey);

        // Matches screenshots: Ai-trace-id header
        if (!req.Headers.Contains(TraceIdHeader))
            req.Headers.Add(TraceIdHeader, Guid.NewGuid().ToString());

        req.Headers.Accept.Clear();
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static T? TryDeserialize<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, JsonOpts); }
        catch { return default; }
    }

    public Task<Operation<PixVerseUploadImageResult>> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Operation<PixVerseUploadImageResult>> UploadImageAsync(string imageUrl, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    private sealed class PixVerseApiEnvelope<T>
    {
        [JsonPropertyName("ErrCode")]
        public int ErrCode { get; init; }

        [JsonPropertyName("ErrMsg")]
        public string? ErrMsg { get; init; }

        [JsonPropertyName("Resp")]
        public T? Resp { get; init; }
    }

    private sealed class PixVerseSubmitResp
    {
        // In the JSON, video_id is a NUMBER (not a string):
        // "video_id": 383655565438332
        [JsonPropertyName("video_id")]
        public long VideoId { get; init; }

        [JsonPropertyName("credits")]
        public int Credits { get; init; }
    }
}
