using Application.PixVerse;
using Application.Result;
using Configuration.PixVerse;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.PixVerse;

public sealed partial class PixVerseService(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<PixVerseService> logger
) : IPixVerseService
{

    private readonly HttpClient _http = httpClient;
    private readonly PixVerseOptions _opt = options.Value;
    private readonly IErrorHandler _error = errorHandler;
    private readonly ILogger<PixVerseService> _logger = logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // -------------------------------------------------
    // Account / Billing
    // -------------------------------------------------
    public async Task<Operation<Balance>> CheckBalanceAsync(CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START PixVerse.CheckBalance", runId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-1 Validate config", runId);
            if (!TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-BAL-1 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<Balance>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-2 Build endpoint. Path={Path}", runId, ApiConstants.BalancePath);
            var endpoint = BuildEndpoint(ApiConstants.BalancePath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-4 Create timeout tokens. HttpTimeout={Timeout}", runId, _opt.HttpTimeout);
            using var timeoutCts = _opt.HttpTimeout > TimeSpan.Zero
                ? new CancellationTokenSource(_opt.HttpTimeout)
                : new CancellationTokenSource();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-5 Send request", runId);
            using var res = await _http.SendAsync(req, linkedCts.Token);

            _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-6 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-BAL-6 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<Balance>(null, $"PixVerse balance failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-7 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(linkedCts.Token);
            _logger.LogDebug("[RUN {RunId}] STEP PV-BAL-7 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-8 Deserialize envelope", runId);
            var env = TryDeserialize<ApiEnvelope<Balance>>(json);
            if (env is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-BAL-8 FAILED Envelope is null", runId);
                return _error.Fail<Balance>(null, "Invalid balance response (null).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-9 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
            if (env.ErrCode != 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-BAL-9 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                return _error.Fail<Balance>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
            }

            if (env.Resp is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-BAL-9 FAILED Resp is null", runId);
                return _error.Fail<Balance>(null, "Invalid balance payload (Resp null).");
            }

            _logger.LogInformation("[RUN {RunId}] SUCCESS PixVerse.CheckBalance", runId);
            return Operation<Balance>.Success(env.Resp, env.ErrMsg);
        }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "[RUN {RunId}] TIMEOUT CheckBalance after {Timeout}", runId, _opt.HttpTimeout);
            return _error.Fail<Balance>(ex, $"Balance check timed out after {_opt.HttpTimeout}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED CheckBalance", runId);
            return _error.Fail<Balance>(ex, "Balance check failed");
        }
    }

    // -------------------------------------------------
    // Text-to-Video
    // -------------------------------------------------
    public async Task<Operation<JobSubmitted>> SubmitTextToVideoAsync(
        TextToVideoRequest request,
        CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START SubmitTextToVideo", runId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-1 Validate request", runId);
            request.Validate();

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-2 Validate config", runId);
            if (!TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-T2V-2 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<JobSubmitted>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-3 Build endpoint. Path={Path}", runId, ApiConstants.TextToVideoPath);
            var endpoint = BuildEndpoint(ApiConstants.TextToVideoPath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-4 Serialize payload", runId);
            var payload = JsonSerializer.Serialize(request, JsonOpts);
            _logger.LogDebug("[RUN {RunId}] STEP PV-T2V-4 PayloadLength={Length}", runId, payload?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-6 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-T2V-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<JobSubmitted>(null, $"Submit failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-8 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-T2V-8 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-9 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<ApiEnvelope<SubmitResp>>(json, JsonOpts);
            if (env is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-T2V-9 FAILED Envelope is null", runId);
                return _error.Fail<JobSubmitted>(null, "Invalid submit response (null).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
            if (env.ErrCode != 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-T2V-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                return _error.Fail<JobSubmitted>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
            }

            if (env.Resp is null || env.Resp.VideoId == 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-T2V-10 FAILED Missing Resp.video_id. VideoId={VideoId}", runId, env.Resp?.VideoId ?? 0);
                return _error.Fail<JobSubmitted>(null, "Invalid submit response (missing Resp.video_id).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-11 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);
            var submitted = new JobSubmitted
            {
                JobId = env.Resp.VideoId,
                Message = env.ErrMsg
            };

            _logger.LogInformation("[RUN {RunId}] SUCCESS SubmitTextToVideo. JobId={JobId}", runId, submitted.JobId);
            return Operation<JobSubmitted>.Success(submitted, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitTextToVideo", runId);
            return _error.Fail<JobSubmitted>(ex, "Submit failed");
        }
    }

    // -------------------------------------------------
    // Image-to-Video
    // -------------------------------------------------
    public async Task<Operation<JobSubmitted>> SubmitImageToVideoAsync(
        ImageToVideoRequest request,
        CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START SubmitImageToVideo", runId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-1 Validate request", runId);
            request.Validate();

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-2 Validate config", runId);
            if (!TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-I2V-2 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<JobSubmitted>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-3 Build endpoint. Path={Path}", runId, ApiConstants.ImageToVideoPath);
            var endpoint = BuildEndpoint(ApiConstants.ImageToVideoPath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-4 Serialize payload", runId);
            var payload = JsonSerializer.Serialize(request, JsonOpts);
            _logger.LogDebug("[RUN {RunId}] STEP PV-I2V-4 PayloadLength={Length}", runId, payload?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-6 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-I2V-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<JobSubmitted>(null, $"SubmitImageToVideo failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-8 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-I2V-8 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-9 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<ApiEnvelope<I2VSubmitResp>>(json, JsonOpts);

            if (env is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-I2V-9 FAILED Envelope is null", runId);
                return _error.Fail<JobSubmitted>(null, "Invalid ImageToVideo response (null).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
            if (env.ErrCode != 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-I2V-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                return _error.Fail<JobSubmitted>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
            }

            if (env.Resp is null || env.Resp.VideoId == 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-I2V-10 FAILED Missing Resp.video_id. VideoId={VideoId}", runId, env.Resp?.VideoId ?? 0);
                return _error.Fail<JobSubmitted>(null, "Invalid ImageToVideo response (missing Resp.video_id).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-I2V-11 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);
            var submitted = new JobSubmitted
            {
                JobId = env.Resp.VideoId,
                Message = env.ErrMsg
            };

            _logger.LogInformation("[RUN {RunId}] SUCCESS SubmitImageToVideo. JobId={JobId}", runId, submitted.JobId);
            return Operation<JobSubmitted>.Success(submitted, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitImageToVideo", runId);
            return _error.Fail<JobSubmitted>(ex, "SubmitImageToVideo failed");
        }
    }

    // -------------------------------------------------
    // Transition
    // -------------------------------------------------
    public async Task<Operation<JobSubmitted>> SubmitTransitionAsync(
        TransitionRequest request,
        CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START SubmitTransition", runId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-1 Validate request", runId);
            request.Validate();

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-2 Validate config", runId);
            if (!TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-TR-2 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<JobSubmitted>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-3 Build endpoint. Path={Path}", runId, ApiConstants.TransitionPath);
            var endpoint = BuildEndpoint(ApiConstants.TransitionPath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-4 Serialize payload", runId);
            var payload = JsonSerializer.Serialize(request, JsonOpts);
            _logger.LogDebug("[RUN {RunId}] STEP PV-TR-4 PayloadLength={Length}", runId, payload?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            // NOTE: Spec says "Ai-Trace-Id" but header matching is case-insensitive; we keep the same header name.
            ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-6 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-TR-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<JobSubmitted>(null, $"SubmitTransition failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-8 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-TR-8 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-9 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<ApiEnvelope<I2VSubmitResp>>(json, JsonOpts);

            if (env is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-TR-9 FAILED Envelope is null", runId);
                return _error.Fail<JobSubmitted>(null, "Invalid Transition response (null).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
            if (env.ErrCode != 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-TR-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                return _error.Fail<JobSubmitted>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
            }

            if (env.Resp is null || env.Resp.VideoId == 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-TR-10 FAILED Missing Resp.video_id. VideoId={VideoId}", runId, env.Resp?.VideoId ?? 0);
                return _error.Fail<JobSubmitted>(null, "Invalid Transition response (missing Resp.video_id).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-TR-11 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);
            var submitted = new JobSubmitted
            {
                JobId = env.Resp.VideoId,
                Message = env.ErrMsg
            };

            _logger.LogInformation("[RUN {RunId}] SUCCESS SubmitTransition. JobId={JobId}", runId, submitted.JobId);
            return Operation<JobSubmitted>.Success(submitted, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitTransition", runId);
            return _error.Fail<JobSubmitted>(ex, "SubmitTransition failed");
        }
    }

    // -------------------------------------------------
    // Status
    // -------------------------------------------------
    public async Task<Operation<GenerationStatus>> GetGenerationStatusAsync(long jobId, CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START GetGenerationStatus. JobId={JobId}", runId, jobId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-1 Validate config", runId);
            if (!TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-ST-1 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<GenerationStatus>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-2 Build endpoint. JobId={JobId}", runId, jobId);
            var endpoint = BuildEndpoint(ApiConstants.StatusPath + Uri.EscapeDataString(jobId.ToString()));

            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-4 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-5 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-ST-5 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<GenerationStatus>(null, $"Status failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-6 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-ST-6 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-7 Try deserialize envelope", runId);
            var env = TryDeserialize<ApiEnvelope<GenerationStatus>>(json);

            if (env is not null)
            {
                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-8 Envelope parsed. ErrCode={ErrCode}", runId, env.ErrCode);

                if (env.ErrCode != 0)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-ST-8 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                    return _error.Fail<GenerationStatus>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                }

                if (env.Resp is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-ST-8 FAILED Resp is null", runId);
                    return _error.Fail<GenerationStatus>(null, "Invalid status payload (Resp null).");
                }

                _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationStatus (envelope)", runId);
                return Operation<GenerationStatus>.Success(env.Resp, env.ErrMsg);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-9 Envelope parse failed. Fallback to raw status model", runId);
            var status = JsonSerializer.Deserialize<GenerationStatus>(json, JsonOpts);

            if (status is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-ST-9 FAILED Status model is null", runId);
                return _error.Fail<GenerationStatus>(null, "Invalid status payload");
            }

            _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationStatus (raw)", runId);
            return Operation<GenerationStatus>.Success(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED GetGenerationStatus. JobId={JobId}", runId, jobId);
            return _error.Fail<GenerationStatus>(ex, "Status check failed");
        }
    }

    // -------------------------------------------------
    // Result
    // -------------------------------------------------
    public async Task<Operation<GenerationResult>> GetGenerationResultAsync(long jobId, CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START GetGenerationResult. JobId={JobId}", runId, jobId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-1 Validate config", runId);
            if (!TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-RS-1 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<GenerationResult>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-2 Build endpoint. JobId={JobId}", runId, jobId);
            var endpoint = BuildEndpoint(ApiConstants.ResultPath + Uri.EscapeDataString(jobId.ToString()));

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-4 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-5 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-RS-5 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<GenerationResult>(null, $"Result failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-6 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-RS-6 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-7 Try deserialize envelope", runId);
            var env = TryDeserialize<ApiEnvelope<GenerationResult>>(json);

            if (env is not null)
            {
                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-8 Envelope parsed. ErrCode={ErrCode}", runId, env.ErrCode);

                if (env.ErrCode != 0)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                    return _error.Fail<GenerationResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                }

                if (env.Resp is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED Resp is null", runId);
                    return _error.Fail<GenerationResult>(null, "Invalid result payload (Resp null).");
                }

                _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (envelope)", runId);
                return Operation<GenerationResult>.Success(env.Resp, env.ErrMsg);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-9 Envelope parse failed. Fallback to raw result model", runId);
            var result = JsonSerializer.Deserialize<GenerationResult>(json, JsonOpts);

            if (result is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-RS-9 FAILED Result model is null", runId);
                return _error.Fail<GenerationResult>(null, "Invalid result payload");
            }

            _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (raw)", runId);
            return Operation<GenerationResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED GetGenerationResult. JobId={JobId}", runId, jobId);
            return _error.Fail<GenerationResult>(ex, "Result check failed");
        }
    }

    // -------------------------------------------------
    // Polling
    // -------------------------------------------------
    public async Task<Operation<GenerationResult>> WaitForCompletionAsync(long jobId, CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START WaitForCompletion. JobId={JobId}", runId, jobId);

        if (jobId == 0)
        {
            _logger.LogWarning("[RUN {RunId}] WaitForCompletion aborted: jobId=0", runId);
            return _error.Business<GenerationResult>("jobId cannot be null or empty.");
        }

        _logger.LogInformation(
            "[RUN {RunId}] STEP PV-POLL-0 Polling settings. Attempts={Attempts} Interval={Interval}",
            runId, _opt.MaxPollingAttempts, _opt.PollingInterval);

        for (var i = 0; i < _opt.MaxPollingAttempts; i++)
        {
            ct.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "STEP PV-4.1 - Poll {Poll}/{Max}. Getting generation status. JobId={JobId}",
                i + 1, _opt.MaxPollingAttempts, jobId);

            var st = await GetGenerationStatusAsync(jobId, ct);

            if (!st.IsSuccessful)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-1 Status call failed. Poll={Poll}/{Max} JobId={JobId}", runId, i + 1, _opt.MaxPollingAttempts, jobId);
                return st.ConvertTo<GenerationResult>();
            }

            if (st.Data is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-2 Invalid status payload (null). Poll={Poll}/{Max} JobId={JobId}", runId, i + 1, _opt.MaxPollingAttempts, jobId);
                return _error.Fail<GenerationResult>(null, "Invalid status payload (null).");
            }

            _logger.LogInformation(
                "[RUN {RunId}] STEP PV-POLL-3 Status received. State={State} IsTerminal={IsTerminal} Poll={Poll}/{Max} JobId={JobId}",
                runId, st.Data.State, st.Data.IsTerminal, i + 1, _opt.MaxPollingAttempts, jobId);

            if (st.Data.IsTerminal)
            {
                if (st.Data.State == JobState.Succeeded)
                {
                    _logger.LogInformation("[RUN {RunId}] STEP PV-POLL-4 Terminal=Succeeded. Fetching result. JobId={JobId}", runId, jobId);
                    return await GetGenerationResultAsync(jobId, ct);
                }

                var msg = $"Job ended with terminal state: {st.Data.State}.";
                _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-4 Terminal!=Succeeded. {Message} JobId={JobId}", runId, msg, jobId);

                return Operation<GenerationResult>.Success(new GenerationResult
                {
                    RawJobId = jobId,
                    RawStatus = (int)st.Data.State
                }, msg);
            }

            _logger.LogDebug("[RUN {RunId}] STEP PV-POLL-5 Delay {Delay} before next poll. Poll={Poll}/{Max} JobId={JobId}",
                runId, _opt.PollingInterval, i + 1, _opt.MaxPollingAttempts, jobId);

            await Task.Delay(_opt.PollingInterval, ct);
        }

        _logger.LogError("[RUN {RunId}] FAILED WaitForCompletion: Polling timed out. JobId={JobId}", runId, jobId);
        return _error.Fail<GenerationResult>(null, "Polling timed out.");
    }

    // -------------------------------------------------
    // Image Upload (file)
    // -------------------------------------------------
    public async Task<Operation<UploadImageResult>> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation(
            "[RUN {RunId}] START UploadImage (file). FileName={FileName} ContentType={ContentType}",
            runId, fileName, contentType);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-1 Validate config", runId);
            if (!TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-1 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<UploadImageResult>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-2 Validate inputs", runId);
            if (imageStream is null)
                return _error.Business<UploadImageResult>("imageStream cannot be null.");

            if (!imageStream.CanRead)
                return _error.Business<UploadImageResult>("imageStream must be readable.");

            if (string.IsNullOrWhiteSpace(fileName))
                return _error.Business<UploadImageResult>("fileName cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(contentType))
                return _error.Business<UploadImageResult>("contentType cannot be null or empty.");

            if (!ApiConstants.AllowedImageMimeTypes.Contains(contentType))
                return _error.Business<UploadImageResult>(
                    $"Unsupported contentType '{contentType}'. Allowed: image/jpeg, image/jpg, image/png, image/webp");

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(ext) || !ApiConstants.AllowedExtensions.Contains(ext))
                return _error.Business<UploadImageResult>(
                    $"Unsupported file extension '{ext}'. Allowed: .png, .webp, .jpeg, .jpg");

            if (imageStream.CanSeek)
            {
                const long maxBytes = 20L * 1024L * 1024L;
                _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-3 Validate size (seekable). MaxBytes={MaxBytes}", runId, maxBytes);

                if (imageStream.Length > maxBytes)
                    return _error.Business<UploadImageResult>("Image file size must be < 20MB.");

                imageStream.Position = 0;
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-4 Build endpoint. Path={Path}", runId, ApiConstants.UploadImagePath);
            var endpoint = BuildEndpoint(ApiConstants.UploadImagePath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-5 Build multipart form", runId);
            using var form = new MultipartFormDataContent();

            var fileContent = new StreamContent(imageStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            form.Add(fileContent, "image", fileName);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-6 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = form };
            ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-7 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-8 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-8 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<UploadImageResult>(null, $"UploadImage failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-9 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-UPF-9 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-10 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<ApiEnvelope<UploadImageResult>>(json, JsonOpts);

            if (env is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-10 FAILED Envelope is null", runId);
                return _error.Fail<UploadImageResult>(null, "Invalid upload response (null).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-11 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
            if (env.ErrCode != 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-11 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                return _error.Fail<UploadImageResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
            }

            if (env.Resp is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-11 FAILED Resp is null", runId);
                return _error.Fail<UploadImageResult>(null, "Invalid upload payload (Resp null).");
            }

            _logger.LogInformation("[RUN {RunId}] SUCCESS UploadImage (file)", runId);
            return Operation<UploadImageResult>.Success(env.Resp, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED UploadImage (file)", runId);
            return _error.Fail<UploadImageResult>(ex, "Upload image failed");
        }
    }

    // -------------------------------------------------
    // Image Upload (url)
    // -------------------------------------------------
    public async Task<Operation<UploadImageResult>> UploadImageAsync(string imageUrl, CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START UploadImage (url). Url={Url}", runId, imageUrl);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-1 Validate config", runId);
            if (!TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-1 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<UploadImageResult>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-2 Validate inputs", runId);
            if (string.IsNullOrWhiteSpace(imageUrl))
                return _error.Business<UploadImageResult>("imageUrl cannot be null or empty.");

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return _error.Business<UploadImageResult>("imageUrl must be a valid http/https absolute URL.");

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-3 Build endpoint. Path={Path}", runId, ApiConstants.UploadImagePath);
            var endpoint = BuildEndpoint(ApiConstants.UploadImagePath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-4 Build multipart form", runId);
            using var form = new MultipartFormDataContent
            {
                { new StringContent(imageUrl, Encoding.UTF8), "image_url" }
            };

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = form };
            ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-6 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<UploadImageResult>(null, $"UploadImage (url) failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-8 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-UPU-8 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-9 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<ApiEnvelope<UploadImageResult>>(json, JsonOpts);

            if (env is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-9 FAILED Envelope is null", runId);
                return _error.Fail<UploadImageResult>(null, "Invalid upload response (null).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
            if (env.ErrCode != 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                return _error.Fail<UploadImageResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
            }

            if (env.Resp is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-10 FAILED Resp is null", runId);
                return _error.Fail<UploadImageResult>(null, "Invalid upload payload (Resp null).");
            }

            _logger.LogInformation("[RUN {RunId}] SUCCESS UploadImage (url)", runId);
            return Operation<UploadImageResult>.Success(env.Resp, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED UploadImage (url)", runId);
            return _error.Fail<UploadImageResult>(ex, "Upload image failed");
        }
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
        var baseUri = new Uri(_opt.BaseUrl.TrimEnd('/') + "/");
        return new Uri(baseUri, pathOrPathWithId.TrimStart('/'));
    }

    private void ApplyAuth(HttpRequestMessage req)
    {
        req.Headers.Remove(ApiConstants.ApiKeyHeader);
        req.Headers.Add(ApiConstants.ApiKeyHeader, _opt.ApiKey);

        if (!req.Headers.Contains(ApiConstants.TraceIdHeader))
            req.Headers.Add(ApiConstants.TraceIdHeader, Guid.NewGuid().ToString());

        req.Headers.Accept.Clear();
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static T? TryDeserialize<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, JsonOpts); }
        catch { return default; }
    }

    public async Task<Operation<JobSubmitted>> SubmitLipSyncAsync(
        LipSyncRequest request,
        CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START SubmitLipSync", runId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-1 Validate request", runId);
            request.Validate();

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-2 Validate config", runId);
            if (!TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-LS-2 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<JobSubmitted>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-3 Build endpoint. Path={Path}", runId, ApiConstants.LipSyncPath);
            var endpoint = BuildEndpoint(ApiConstants.LipSyncPath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-4 Serialize payload", runId);
            var payload = JsonSerializer.Serialize(request, JsonOpts);
            _logger.LogDebug("[RUN {RunId}] STEP PV-LS-4 PayloadLength={Length}", runId, payload?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            // Must include API-KEY + Ai-Trace-Id (already done by ApplyAuth)
            ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-6 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-LS-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<JobSubmitted>(null, $"SubmitLipSync failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-8 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-LS-8 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-9 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<ApiEnvelope<SubmitResp>>(json, JsonOpts);
            if (env is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-LS-9 FAILED Envelope is null", runId);
                return _error.Fail<JobSubmitted>(null, "Invalid LipSync response (null).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
            if (env.ErrCode != 0)
            {
                _logger.LogWarning(
                    "[RUN {RunId}] STEP PV-LS-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}",
                    runId, env.ErrCode, env.ErrMsg);

                return _error.Fail<JobSubmitted>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
            }

            if (env.Resp is null || env.Resp.VideoId == 0)
            {
                _logger.LogWarning(
                    "[RUN {RunId}] STEP PV-LS-10 FAILED Missing Resp.video_id. VideoId={VideoId}",
                    runId, env.Resp?.VideoId ?? 0);

                return _error.Fail<JobSubmitted>(null, "Invalid LipSync response (missing Resp.video_id).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-11 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);
            var submitted = new JobSubmitted
            {
                JobId = env.Resp.VideoId,
                Message = env.ErrMsg
            };

            _logger.LogInformation("[RUN {RunId}] SUCCESS SubmitLipSync. JobId={JobId}", runId, submitted.JobId);
            return Operation<JobSubmitted>.Success(submitted, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitLipSync", runId);
            return _error.Fail<JobSubmitted>(ex, "SubmitLipSync failed");
        }
    }

}
