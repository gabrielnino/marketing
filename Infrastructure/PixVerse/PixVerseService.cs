using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
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
) : BaseVerseService(options.Value), IPixVerseService
{

    private readonly HttpClient _http = httpClient;
    private readonly PixVerseOptions _opt = options.Value;
    private readonly IErrorHandler _error = errorHandler;
    private readonly ILogger<PixVerseService> _logger = logger;



    public async Task<Operation<GenerationStatus>> GetGenerationStatusAsync(long jobId, CancellationToken ct = default)
    {
        var runId = VerseApiSupport.NewRunId();
        _logger.LogInformation("[RUN {RunId}] START GetGenerationStatus. JobId={JobId}", runId, jobId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-1 Validate config", runId);
            if (!Helper.TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-ST-1 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<GenerationStatus>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-2 Build endpoint. JobId={JobId}", runId, jobId);
            var endpoint = Helper.BuildEndpoint(ApiConstants.StatusPath + Uri.EscapeDataString(jobId.ToString()));

            _logger.LogInformation("[RUN {RunId}] STEP PV-ST-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            Helper.ApplyAuth(req);

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
            var env = Helper.TryDeserialize<ApiEnvelope<GenerationStatus>>(json);

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
    public async Task<Operation<Generation>> GetGenerationResultAsync(long jobId, CancellationToken ct = default)
    {
        var runId = VerseApiSupport.NewRunId();
        _logger.LogInformation("[RUN {RunId}] START GetGenerationResult. JobId={JobId}", runId, jobId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-1 Validate config", runId);
            if (!Helper.TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-RS-1 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<Generation>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-2 Build endpoint. JobId={JobId}", runId, jobId);
            var endpoint = Helper.BuildEndpoint(ApiConstants.ResultPath + Uri.EscapeDataString(jobId.ToString()));

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            Helper.ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-4 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-5 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-RS-5 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<Generation>(null, $"Result failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-6 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-RS-6 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-7 Try deserialize envelope", runId);
            var env = Helper.TryDeserialize<ApiEnvelope<Generation>>(json);

            if (env is not null)
            {
                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-8 Envelope parsed. ErrCode={ErrCode}", runId, env.ErrCode);

                if (env.ErrCode != 0)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                    return _error.Fail<Generation>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                }

                if (env.Resp is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED Resp is null", runId);
                    return _error.Fail<Generation>(null, "Invalid result payload (Resp null).");
                }

                _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (envelope)", runId);
                return Operation<Generation>.Success(env.Resp, env.ErrMsg);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-RS-9 Envelope parse failed. Fallback to raw result model", runId);
            var result = JsonSerializer.Deserialize<Generation>(json, JsonOpts);

            if (result is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-RS-9 FAILED Result model is null", runId);
                return _error.Fail<Generation>(null, "Invalid result payload");
            }

            _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (raw)", runId);
            return Operation<Generation>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED GetGenerationResult. JobId={JobId}", runId, jobId);
            return _error.Fail<Generation>(ex, "Result check failed");
        }
    }

    // -------------------------------------------------
    // Polling
    // -------------------------------------------------
    public async Task<Operation<Generation>> WaitForCompletionAsync(long jobId, CancellationToken ct = default)
    {
        var runId = VerseApiSupport.NewRunId();
        _logger.LogInformation("[RUN {RunId}] START WaitForCompletion. JobId={JobId}", runId, jobId);

        if (jobId == 0)
        {
            _logger.LogWarning("[RUN {RunId}] WaitForCompletion aborted: jobId=0", runId);
            return _error.Business<Generation>("jobId cannot be null or empty.");
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
                return st.ConvertTo<Generation>();
            }

            if (st.Data is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-2 Invalid status payload (null). Poll={Poll}/{Max} JobId={JobId}", runId, i + 1, _opt.MaxPollingAttempts, jobId);
                return _error.Fail<Generation>(null, "Invalid status payload (null).");
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

                return Operation<Generation>.Success(new Generation
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
        return _error.Fail<Generation>(null, "Polling timed out.");
    }

    // -------------------------------------------------
    // Image Upload (file)
    // -------------------------------------------------
    public async Task<Operation<UploadImage>> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var runId = VerseApiSupport.NewRunId();
        _logger.LogInformation(
            "[RUN {RunId}] START UploadImage (file). FileName={FileName} ContentType={ContentType}",
            runId, fileName, contentType);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-1 Validate config", runId);
            if (!Helper.TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-1 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<UploadImage>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-2 Validate inputs", runId);
            if (imageStream is null)
                return _error.Business<UploadImage>("imageStream cannot be null.");

            if (!imageStream.CanRead)
                return _error.Business<UploadImage>("imageStream must be readable.");

            if (string.IsNullOrWhiteSpace(fileName))
                return _error.Business<UploadImage>("fileName cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(contentType))
                return _error.Business<UploadImage>("contentType cannot be null or empty.");

            if (!ApiConstants.AllowedImageMimeTypes.Contains(contentType))
                return _error.Business<UploadImage>(
                    $"Unsupported contentType '{contentType}'. Allowed: image/jpeg, image/jpg, image/png, image/webp");

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(ext) || !ApiConstants.AllowedExtensions.Contains(ext))
                return _error.Business<UploadImage>(
                    $"Unsupported file extension '{ext}'. Allowed: .png, .webp, .jpeg, .jpg");

            if (imageStream.CanSeek)
            {
                const long maxBytes = 20L * 1024L * 1024L;
                _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-3 Validate size (seekable). MaxBytes={MaxBytes}", runId, maxBytes);

                if (imageStream.Length > maxBytes)
                    return _error.Business<UploadImage>("Image file size must be < 20MB.");

                imageStream.Position = 0;
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-4 Build endpoint. Path={Path}", runId, ApiConstants.UploadImagePath);
            var endpoint = Helper.BuildEndpoint(ApiConstants.UploadImagePath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-5 Build multipart form", runId);
            using var form = new MultipartFormDataContent();

            var fileContent = new StreamContent(imageStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            form.Add(fileContent, "image", fileName);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-6 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = form };
            Helper.ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-7 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-8 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-8 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<UploadImage>(null, $"UploadImage failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-9 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-UPF-9 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-10 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<ApiEnvelope<UploadImage>>(json, JsonOpts);

            if (env is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-10 FAILED Envelope is null", runId);
                return _error.Fail<UploadImage>(null, "Invalid upload response (null).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-11 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
            if (env.ErrCode != 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-11 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                return _error.Fail<UploadImage>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
            }

            if (env.Resp is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPF-11 FAILED Resp is null", runId);
                return _error.Fail<UploadImage>(null, "Invalid upload payload (Resp null).");
            }

            _logger.LogInformation("[RUN {RunId}] SUCCESS UploadImage (file)", runId);
            return Operation<UploadImage>.Success(env.Resp, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED UploadImage (file)", runId);
            return _error.Fail<UploadImage>(ex, "Upload image failed");
        }
    }

    // -------------------------------------------------
    // Image Upload (url)
    // -------------------------------------------------
    public async Task<Operation<UploadImage>> UploadImageAsync(string imageUrl, CancellationToken ct = default)
    {
        var runId = VerseApiSupport.NewRunId();
        _logger.LogInformation("[RUN {RunId}] START UploadImage (url). Url={Url}", runId, imageUrl);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-1 Validate config", runId);
            if (!Helper.TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-1 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<UploadImage>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-2 Validate inputs", runId);
            if (string.IsNullOrWhiteSpace(imageUrl))
                return _error.Business<UploadImage>("imageUrl cannot be null or empty.");

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return _error.Business<UploadImage>("imageUrl must be a valid http/https absolute URL.");

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-3 Build endpoint. Path={Path}", runId, ApiConstants.UploadImagePath);
            var endpoint = Helper.BuildEndpoint(ApiConstants.UploadImagePath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-4 Build multipart form", runId);
            using var form = new MultipartFormDataContent
            {
                { new StringContent(imageUrl, Encoding.UTF8), "image_url" }
            };

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = form };
            Helper.ApplyAuth(req);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-6 Send request", runId);
            using var res = await _http.SendAsync(req, ct);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-7 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-7 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                return _error.Fail<UploadImage>(null, $"UploadImage (url) failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-8 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-UPU-8 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-9 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<ApiEnvelope<UploadImage>>(json, JsonOpts);

            if (env is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-9 FAILED Envelope is null", runId);
                return _error.Fail<UploadImage>(null, "Invalid upload response (null).");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
            if (env.ErrCode != 0)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                return _error.Fail<UploadImage>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
            }

            if (env.Resp is null)
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-UPU-10 FAILED Resp is null", runId);
                return _error.Fail<UploadImage>(null, "Invalid upload payload (Resp null).");
            }

            _logger.LogInformation("[RUN {RunId}] SUCCESS UploadImage (url)", runId);
            return Operation<UploadImage>.Success(env.Resp, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED UploadImage (url)", runId);
            return _error.Fail<UploadImage>(ex, "Upload image failed");
        }
    }



    public async Task<Operation<JobSubmitted>> SubmitLipSyncAsync(
        LipSync request,
        CancellationToken ct = default)
    {
        var runId = VerseApiSupport.NewRunId();
        _logger.LogInformation("[RUN {RunId}] START SubmitLipSync", runId);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-1 Validate request", runId);
            request.Validate();

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-2 Validate config", runId);
            if (!Helper.TryValidateConfig(out var configError))
            {
                _logger.LogWarning("[RUN {RunId}] STEP PV-LS-2 FAILED Config invalid: {Error}", runId, configError);
                return _error.Fail<JobSubmitted>(null, configError);
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-3 Build endpoint. Path={Path}", runId, ApiConstants.LipSyncPath);
            var endpoint = Helper.BuildEndpoint(ApiConstants.LipSyncPath);

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-4 Serialize payload", runId);
            var payload = JsonSerializer.Serialize(request, JsonOpts);
            _logger.LogDebug("[RUN {RunId}] STEP PV-LS-4 PayloadLength={Length}", runId, payload?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-LS-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            // Must include API-KEY + Ai-Trace-Id (already done by ApplyAuth)
            Helper.ApplyAuth(req);

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

    public async Task<Operation<FileInfo>> DownloadVideoAsync(
        long jobId,
        string destinationFilePath,
        int videoIndex = 0,
        CancellationToken ct = default)
    {
        var runId = VerseApiSupport.NewRunId();
        _logger.LogInformation(
            "[RUN {RunId}] START PixVerse.DownloadVideo jobId={JobId} videoIndex={VideoIndex} dest={Dest}",
            runId, jobId, videoIndex, destinationFilePath);

        try
        {
            if (jobId == 0)
                return _error.Business<FileInfo>("jobId cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(destinationFilePath))
                return _error.Business<FileInfo>("destinationFilePath cannot be null or empty.");

            if (videoIndex < 0)
                return _error.Business<FileInfo>("videoIndex cannot be negative.");

            // 1) Get result -> video URL
            _logger.LogInformation("[RUN {RunId}] STEP 1: Fetch generation result", runId);
            var resOp = await GetGenerationResultAsync(jobId, ct);

            if (!resOp.IsSuccessful|| resOp.Data is null)
                return _error.Fail<FileInfo>(null, $"Cannot download video because generation result is not available. jobId={jobId}");

            var result = resOp.Data;

            if (result.VideoUrls is null || result.VideoUrls.Count == 0)
                return _error.Fail<FileInfo>(null, $"No video URLs found for jobId={jobId}");

            if (videoIndex >= result.VideoUrls.Count)
                return _error.Fail<FileInfo>(
                    null,
                    $"videoIndex out of range. videoIndex={videoIndex}, available={result.VideoUrls.Count}, jobId={jobId}");

            var videoUrl = result.VideoUrls[videoIndex];

            if (string.IsNullOrWhiteSpace(videoUrl))
                return _error.Fail<FileInfo>(null, $"Video URL is empty for jobId={jobId}, videoIndex={videoIndex}");

            _logger.LogInformation("[RUN {RunId}] STEP 2: Selected videoUrl={VideoUrl}", runId, videoUrl);

            // 2) Resolve final path (allow passing a folder)
            string finalPath;
            if (Directory.Exists(destinationFilePath) ||
                destinationFilePath.EndsWith(Path.DirectorySeparatorChar) ||
                destinationFilePath.EndsWith(Path.AltDirectorySeparatorChar))
            {
                var fileName = $"{jobId}_{videoIndex}.mp4";
                finalPath = Path.Combine(destinationFilePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), fileName);
            }
            else
            {
                finalPath = destinationFilePath;
                if (string.IsNullOrEmpty(Path.GetExtension(finalPath)))
                    finalPath += ".mp4";
            }

            var dir = Path.GetDirectoryName(finalPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var tmpPath = finalPath + ".download.tmp";

            // 3) Download streaming -> temp file
            _logger.LogInformation("[RUN {RunId}] STEP 3: Downloading to temp file tmp={Tmp}", runId, tmpPath);

            using (var req = new HttpRequestMessage(HttpMethod.Get, videoUrl))
            {
                // Some CDNs are public; if PixVerse requires auth for asset URLs, apply here:
                // ApplyAuth(req);

                using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

                if (!resp.IsSuccessStatusCode)
                    return _error.Fail<FileInfo>(
                        null,
                        $"PixVerse video download failed. HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");

                await using var httpStream = await resp.Content.ReadAsStreamAsync(ct);
                await using var fileStream = new FileStream(
                    tmpPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

                await httpStream.CopyToAsync(fileStream, ct);
                await fileStream.FlushAsync(ct);
            }

            // 4) Atomically move temp -> final (best-effort)
            _logger.LogInformation("[RUN {RunId}] STEP 4: Moving temp to final final={Final}", runId, finalPath);

            if (File.Exists(finalPath))
                File.Delete(finalPath);

            File.Move(tmpPath, finalPath);

            var fi = new FileInfo(finalPath);
            _logger.LogInformation(
                "[RUN {RunId}] SUCCESS PixVerse.DownloadVideo saved={Path} bytes={Bytes}",
                runId, fi.FullName, fi.Length);

            return Operation<FileInfo>.Success(fi);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[RUN {RunId}] CANCELED PixVerse.DownloadVideo", runId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED PixVerse.DownloadVideo", runId);

            // best-effort cleanup of tmp file
            try
            {
                var tmp = destinationFilePath + ".download.tmp";
                if (File.Exists(tmp))
                    File.Delete(tmp);
            }
            catch { /* ignore */ }

            return _error.Fail<FileInfo>(ex, $"PixVerse video download failed for jobId={jobId}");
        }
    }
}
