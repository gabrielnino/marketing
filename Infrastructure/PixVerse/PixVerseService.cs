using Application.PixVerse;
using Application.Result;
using Configuration.PixVerse;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.PixVerse;

public sealed class PixVerseService(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<PixVerseService> logger
) : IPixVerseService
{
    // -----------------------------
    // PixVerse API paths (Infrastructure knowledge)
    // -----------------------------
    private const string BalancePath = "/v1/balance";
    private const string TextToVideoPath = "/v1/video/text";
    private const string StatusPath = "/v1/video/status/";
    private const string ResultPath = "/v1/video/result/";

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
            if (string.IsNullOrWhiteSpace(_opt.BaseUrl))
                return _error.Fail<PixVerseBalance>(null, "PixVerse BaseUrl is not configured.");

            if (string.IsNullOrWhiteSpace(_opt.ApiKey))
                return _error.Fail<PixVerseBalance>(null, "PixVerse ApiKey is not configured.");

            var endpoint = new Uri(
                new Uri(_opt.BaseUrl.TrimEnd('/') + "/"),
                BalancePath.TrimStart('/')
            );

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);

            // ✅ Bearer authorization using _opt.ApiKey
            req.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _opt.ApiKey);

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
            request.Validate();

            var payload = JsonSerializer.Serialize(request, JsonOpts);

            if (string.IsNullOrWhiteSpace(_opt.BaseUrl))
                return _error.Fail<PixVerseJobSubmitted>(null, "PixVerse BaseUrl is not configured.");

            using var req = new HttpRequestMessage(HttpMethod.Post, TextToVideoPath)
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
            var submitted = JsonSerializer.Deserialize<PixVerseJobSubmitted>(json, JsonOpts);

            if (submitted is null || string.IsNullOrWhiteSpace(submitted.JobId))
                return _error.Fail<PixVerseJobSubmitted>(null, "Invalid submit response");

            return Operation<PixVerseJobSubmitted>.Success(submitted);
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
        using var req = new HttpRequestMessage(
            HttpMethod.Get,
            StatusPath + Uri.EscapeDataString(jobId));

        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
            return _error.Fail<PixVerseGenerationStatus>(
                null,
                $"Status failed. HTTP {(int)res.StatusCode}");

        var json = await res.Content.ReadAsStringAsync(ct);
        var status = JsonSerializer.Deserialize<PixVerseGenerationStatus>(json, JsonOpts);

        return status is null
            ? _error.Fail<PixVerseGenerationStatus>(null, "Invalid status payload")
            : Operation<PixVerseGenerationStatus>.Success(status);
    }

    public async Task<Operation<PixVerseGenerationResult>> GetGenerationResultAsync(
        string jobId,
        CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(
            HttpMethod.Get,
            ResultPath + Uri.EscapeDataString(jobId));

        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
            return _error.Fail<PixVerseGenerationResult>(
                null,
                $"Result failed. HTTP {(int)res.StatusCode}");

        var json = await res.Content.ReadAsStringAsync(ct);
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

            // Case A: status call failed -> propagate same failure typed as PixVerseGenerationResult
            if (!st.IsSuccessful)
            {
                _logger.LogWarning(
                    "STEP PV-4.1 - Status call failed. JobId={JobId}. Type={Type}. Message={Message}",
                    jobId, st.Type, st.Message);

                return st.ConvertTo<PixVerseGenerationResult>(); // <-- FIX (was Convert<>)
            }

            // Case B: success but null payload -> explicit failure (ConvertTo would throw)
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

                // If succeeded, fetch full result
                if (st.Data.State == PixVerseJobState.Succeeded)
                    return await GetGenerationResultAsync(jobId, ct);

                // If failed/cancelled, return a result-shaped failure payload
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

    private static string NewRunId() =>
        Guid.NewGuid().ToString("N")[..8];

    private void ApplyAuth(HttpRequestMessage req)
    {
        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _opt.ApiKey);
    }
}
