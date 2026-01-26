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

    private const string BalancePath = "/openapi/v2/account/balance";
    private const string TextToVideoPath = "/openapi/v2/video/text/generate";
    private const string ImageToVideoPath = "/openapi/v2/video/img/generate";
    private const string TransitionPath = "/openapi/v2/video/transition/generate";
    private const string StatusPath = "/openapi/v2/video/status/";
    private const string ResultPath = "/openapi/v2/video/result/";
    private const string UploadImagePath = "/openapi/v2/image/upload";

    private const string ApiKeyHeader = "API-KEY";
    private const string TraceIdHeader = "Ai-trace-id";

    private static readonly HashSet<string> AllowedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpeg", ".jpg", ".png", ".webp"
    };

    private readonly HttpClient _http = httpClient;
    private readonly PixVerseOptions _opt = options.Value;
    private readonly IErrorHandler _error = errorHandler;
    private readonly ILogger<PixVerseService> _logger = logger;

    private static readonly JsonSerializerOptions JsonOpts =
        new(JsonSerializerDefaults.Web);


    public async Task<Operation<Balance>> CheckBalanceAsync(CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START PixVerse.CheckBalance", runId);

        try
        {
            if (!TryValidateConfig(out var configError))
                return _error.Fail<Balance>(null, configError);

            var endpoint = BuildEndpoint(BalancePath);

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            ApplyAuth(req);

            using var timeoutCts = _opt.HttpTimeout > TimeSpan.Zero
                ? new CancellationTokenSource(_opt.HttpTimeout)
                : new CancellationTokenSource();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            using var res = await _http.SendAsync(req, linkedCts.Token);

            if (!res.IsSuccessStatusCode)
                return _error.Fail<Balance>(null, $"PixVerse balance failed. HTTP {(int)res.StatusCode}");

            var json = await res.Content.ReadAsStringAsync(linkedCts.Token);

            var env = TryDeserialize<ApiEnvelope<Balance>>(json);
            if (env is null)
                return _error.Fail<Balance>(null, "Invalid balance response (null).");

            if (env.ErrCode != 0)
                return _error.Fail<Balance>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            if (env.Resp is null)
                return _error.Fail<Balance>(null, "Invalid balance payload (Resp null).");

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

    public async Task<Operation<JobSubmitted>> SubmitTextToVideoAsync(
        TextToVideoRequest request,
        CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START SubmitTextToVideo", runId);

        try
        {
            request.Validate();

            if (!TryValidateConfig(out var configError))
                return _error.Fail<JobSubmitted>(null, configError);

            var endpoint = BuildEndpoint(TextToVideoPath);
            var payload = JsonSerializer.Serialize(request, JsonOpts);

            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            ApplyAuth(req);

            using var res = await _http.SendAsync(req, ct);

            if (!res.IsSuccessStatusCode)
                return _error.Fail<JobSubmitted>(null, $"Submit failed. HTTP {(int)res.StatusCode}");

            var json = await res.Content.ReadAsStringAsync(ct);

            var env = JsonSerializer.Deserialize<ApiEnvelope<SubmitResp>>(json, JsonOpts);

            if (env is null)
                return _error.Fail<JobSubmitted>(null, "Invalid submit response (null).");

            if (env.ErrCode != 0)
                return _error.Fail<JobSubmitted>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            if (env.Resp is null || env.Resp.VideoId == 0)
                return _error.Fail<JobSubmitted>(null, "Invalid submit response (missing Resp.video_id).");

            var submitted = new JobSubmitted
            {
                JobId = env.Resp.VideoId,
                Message = env.ErrMsg
            };

            return Operation<JobSubmitted>.Success(submitted, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitTextToVideo", runId);
            return _error.Fail<JobSubmitted>(ex, "Submit failed");
        }
    }


    public async Task<Operation<JobSubmitted>> SubmitImageToVideoAsync(
        ImageToVideoRequest request,
        CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START SubmitImageToVideo", runId);

        try
        {
            request.Validate();

            if (!TryValidateConfig(out var configError))
                return _error.Fail<JobSubmitted>(null, configError);

            var endpoint = BuildEndpoint(ImageToVideoPath);
            var payload = JsonSerializer.Serialize(request, JsonOpts);

            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            ApplyAuth(req);

            using var res = await _http.SendAsync(req, ct);

            if (!res.IsSuccessStatusCode)
                return _error.Fail<JobSubmitted>(null, $"SubmitImageToVideo failed. HTTP {(int)res.StatusCode}");

            var json = await res.Content.ReadAsStringAsync(ct);

            var env = JsonSerializer.Deserialize<ApiEnvelope<I2VSubmitResp>>(json, JsonOpts);

            if (env is null)
                return _error.Fail<JobSubmitted>(null, "Invalid ImageToVideo response (null).");

            if (env.ErrCode != 0)
                return _error.Fail<JobSubmitted>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            if (env.Resp is null || env.Resp.VideoId == 0)
                return _error.Fail<JobSubmitted>(null, "Invalid ImageToVideo response (missing Resp.video_id).");

            var submitted = new JobSubmitted
            {
                JobId = env.Resp.VideoId,
                Message = env.ErrMsg
            };

            return Operation<JobSubmitted>.Success(submitted, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitImageToVideo", runId);
            return _error.Fail<JobSubmitted>(ex, "SubmitImageToVideo failed");
        }
    }


    public async Task<Operation<JobSubmitted>> SubmitTransitionAsync(
        TransitionRequest request,
        CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START SubmitTransition", runId);

        try
        {
            request.Validate();

            if (!TryValidateConfig(out var configError))
                return _error.Fail<JobSubmitted>(null, configError);

            var endpoint = BuildEndpoint(TransitionPath);
            var payload = JsonSerializer.Serialize(request, JsonOpts);

            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            // NOTE: Spec says "Ai-Trace-Id" but header matching is case-insensitive; we keep the same header name.
            ApplyAuth(req);

            using var res = await _http.SendAsync(req, ct);

            if (!res.IsSuccessStatusCode)
                return _error.Fail<JobSubmitted>(null, $"SubmitTransition failed. HTTP {(int)res.StatusCode}");

            var json = await res.Content.ReadAsStringAsync(ct);

            var env = JsonSerializer.Deserialize<ApiEnvelope<I2VSubmitResp>>(json, JsonOpts);

            if (env is null)
                return _error.Fail<JobSubmitted>(null, "Invalid Transition response (null).");

            if (env.ErrCode != 0)
                return _error.Fail<JobSubmitted>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            if (env.Resp is null || env.Resp.VideoId == 0)
                return _error.Fail<JobSubmitted>(null, "Invalid Transition response (missing Resp.video_id).");

            var submitted = new JobSubmitted
            {
                JobId = env.Resp.VideoId,
                Message = env.ErrMsg
            };

            return Operation<JobSubmitted>.Success(submitted, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitTransition", runId);
            return _error.Fail<JobSubmitted>(ex, "SubmitTransition failed");
        }
    }

    public async Task<Operation<GenerationStatus>> GetGenerationStatusAsync(long jobId, CancellationToken ct = default)
    {
        if (!TryValidateConfig(out var configError))
            return _error.Fail<GenerationStatus>(null, configError);

        var endpoint = BuildEndpoint(StatusPath + Uri.EscapeDataString(jobId.ToString()));

        using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
            return _error.Fail<GenerationStatus>(null, $"Status failed. HTTP {(int)res.StatusCode}");

        var json = await res.Content.ReadAsStringAsync(ct);

        var env = TryDeserialize<ApiEnvelope<GenerationStatus>>(json);
        if (env is not null)
        {
            if (env.ErrCode != 0)
                return _error.Fail<GenerationStatus>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            return env.Resp is null
                ? _error.Fail<GenerationStatus>(null, "Invalid status payload (Resp null).")
                : Operation<GenerationStatus>.Success(env.Resp, env.ErrMsg);
        }

        var status = JsonSerializer.Deserialize<GenerationStatus>(json, JsonOpts);

        return status is null
            ? _error.Fail<GenerationStatus>(null, "Invalid status payload")
            : Operation<GenerationStatus>.Success(status);
    }

    public async Task<Operation<GenerationResult>> GetGenerationResultAsync(long jobId, CancellationToken ct = default)
    {
        if (!TryValidateConfig(out var configError))
            return _error.Fail<GenerationResult>(null, configError);

        var endpoint = BuildEndpoint(ResultPath + Uri.EscapeDataString(jobId.ToString()));

        using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
            return _error.Fail<GenerationResult>(null, $"Result failed. HTTP {(int)res.StatusCode}");

        var json = await res.Content.ReadAsStringAsync(ct);

        var env = TryDeserialize<ApiEnvelope<GenerationResult>>(json);
        if (env is not null)
        {
            if (env.ErrCode != 0)
                return _error.Fail<GenerationResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            return env.Resp is null
                ? _error.Fail<GenerationResult>(null, "Invalid result payload (Resp null).")
                : Operation<GenerationResult>.Success(env.Resp, env.ErrMsg);
        }

        var result = JsonSerializer.Deserialize<GenerationResult>(json, JsonOpts);

        return result is null
            ? _error.Fail<GenerationResult>(null, "Invalid result payload")
            : Operation<GenerationResult>.Success(result);
    }

    public async Task<Operation<GenerationResult>> WaitForCompletionAsync(long jobId, CancellationToken ct = default)
    {
        if (jobId == 0)
            return _error.Business<GenerationResult>("jobId cannot be null or empty.");

        for (var i = 0; i < _opt.MaxPollingAttempts; i++)
        {
            ct.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "STEP PV-4.1 - Poll {Poll}/{Max}. Getting generation status. JobId={JobId}",
                i + 1, _opt.MaxPollingAttempts, jobId);

            var st = await GetGenerationStatusAsync(jobId, ct);

            if (!st.IsSuccessful)
                return st.ConvertTo<GenerationResult>();

            if (st.Data is null)
                return _error.Fail<GenerationResult>(null, "Invalid status payload (null).");

            if (st.Data.IsTerminal)
            {
                if (st.Data.State == JobState.Succeeded)
                    return await GetGenerationResultAsync(jobId, ct);

                var msg = $"Job ended with terminal state: {st.Data.State}.";
                return Operation<GenerationResult>.Success(new GenerationResult
                {
                    RawJobId = jobId,
                    RawStatus = (int)st.Data.State
                }, msg);
            }

            await Task.Delay(_opt.PollingInterval, ct);
        }

        return _error.Fail<GenerationResult>(null, "Polling timed out.");
    }

    public async Task<Operation<UploadImageResult>> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken ct = default)
    {

        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START UploadImage (file). FileName={FileName} ContentType={ContentType}", runId, fileName, contentType);

        try
        {
            if (!TryValidateConfig(out var configError))
                return _error.Fail<UploadImageResult>(null, configError);

            if (imageStream is null)
                return _error.Business<UploadImageResult>("imageStream cannot be null.");

            if (!imageStream.CanRead)
                return _error.Business<UploadImageResult>("imageStream must be readable.");

            if (string.IsNullOrWhiteSpace(fileName))
                return _error.Business<UploadImageResult>("fileName cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(contentType))
                return _error.Business<UploadImageResult>("contentType cannot be null or empty.");

            if (!AllowedImageMimeTypes.Contains(contentType))
                return _error.Business<UploadImageResult>(
                    $"Unsupported contentType '{contentType}'. Allowed: image/jpeg, image/jpg, image/png, image/webp");

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
                return _error.Business<UploadImageResult>(
                    $"Unsupported file extension '{ext}'. Allowed: .png, .webp, .jpeg, .jpg");

            if (imageStream.CanSeek)
            {
                const long maxBytes = 20L * 1024L * 1024L;
                if (imageStream.Length > maxBytes)
                    return _error.Business<UploadImageResult>("Image file size must be < 20MB.");

                imageStream.Position = 0;
            }

            var endpoint = BuildEndpoint(UploadImagePath);

            using var form = new MultipartFormDataContent();

            var fileContent = new StreamContent(imageStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            form.Add(fileContent, "image", fileName);

            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = form };
            ApplyAuth(req);

            using var res = await _http.SendAsync(req, ct);

            if (!res.IsSuccessStatusCode)
                return _error.Fail<UploadImageResult>(null, $"UploadImage failed. HTTP {(int)res.StatusCode}");

            var json = await res.Content.ReadAsStringAsync(ct);

            var env = JsonSerializer.Deserialize<ApiEnvelope<UploadImageResult>>(json, JsonOpts);

            if (env is null)
                return _error.Fail<UploadImageResult>(null, "Invalid upload response (null).");

            if (env.ErrCode != 0)
                return _error.Fail<UploadImageResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            if (env.Resp is null)
                return _error.Fail<UploadImageResult>(null, "Invalid upload payload (Resp null).");

            return Operation<UploadImageResult>.Success(env.Resp, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED UploadImage (file)", runId);
            return _error.Fail<UploadImageResult>(ex, "Upload image failed");
        }
    }

    public async Task<Operation<UploadImageResult>> UploadImageAsync(string imageUrl, CancellationToken ct = default)
    {

        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START UploadImage (url). Url={Url}", runId, imageUrl);

        try
        {
            if (!TryValidateConfig(out var configError))
                return _error.Fail<UploadImageResult>(null, configError);

            if (string.IsNullOrWhiteSpace(imageUrl))
                return _error.Business<UploadImageResult>("imageUrl cannot be null or empty.");

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return _error.Business<UploadImageResult>("imageUrl must be a valid http/https absolute URL.");

            var endpoint = BuildEndpoint(UploadImagePath);

            using var form = new MultipartFormDataContent
            {
                { new StringContent(imageUrl, Encoding.UTF8), "image_url" }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = form };
            ApplyAuth(req);

            using var res = await _http.SendAsync(req, ct);

            if (!res.IsSuccessStatusCode)
                return _error.Fail<UploadImageResult>(null, $"UploadImage (url) failed. HTTP {(int)res.StatusCode}");

            var json = await res.Content.ReadAsStringAsync(ct);

            var env = JsonSerializer.Deserialize<ApiEnvelope<UploadImageResult>>(json, JsonOpts);

            if (env is null)
                return _error.Fail<UploadImageResult>(null, "Invalid upload response (null).");

            if (env.ErrCode != 0)
                return _error.Fail<UploadImageResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");

            if (env.Resp is null)
                return _error.Fail<UploadImageResult>(null, "Invalid upload payload (Resp null).");

            return Operation<UploadImageResult>.Success(env.Resp, env.ErrMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RUN {RunId}] FAILED UploadImage (url)", runId);
            return _error.Fail<UploadImageResult>(ex, "Upload image failed");
        }
    }


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
        req.Headers.Remove(ApiKeyHeader);
        req.Headers.Add(ApiKeyHeader, _opt.ApiKey);

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
}
