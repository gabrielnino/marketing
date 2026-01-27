using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.PixVerse;

public sealed partial class ImageClient(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<ImageClient> logger
) : PixVerseBase(options.Value), IImageClient
{

    private readonly HttpClient _http = httpClient;
    private readonly IErrorHandler _error = errorHandler;
    private readonly ILogger<ImageClient> _logger = logger;


    public async Task<Operation<UploadImage>> UploadAsync(
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

            if (!Api.AllowedImageMimeTypes.Contains(contentType))
                return _error.Business<UploadImage>(
                    $"Unsupported contentType '{contentType}'. Allowed: image/jpeg, image/jpg, image/png, image/webp");

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(ext) || !Api.AllowedExtensions.Contains(ext))
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

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-4 Build endpoint. Path={Path}", runId, Api.UploadImagePath);
            var endpoint = BuildEndpoint(Api.UploadImagePath);

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
                return _error.Fail<UploadImage>(null, $"UploadImage failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-9 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-UPF-9 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPF-10 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<Envelope<UploadImage>>(json, JsonOpts);

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

    public async Task<Operation<UploadImage>> UploadAsync(string imageUrl, CancellationToken ct = default)
    {
        var runId = NewRunId();
        _logger.LogInformation("[RUN {RunId}] START UploadImage (url). Url={Url}", runId, imageUrl);

        try
        {
            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-1 Validate config", runId);
            if (!TryValidateConfig(out var configError))
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

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-3 Build endpoint. Path={Path}", runId, Api.UploadImagePath);
            var endpoint = BuildEndpoint(Api.UploadImagePath);

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
                return _error.Fail<UploadImage>(null, $"UploadImage (url) failed. HTTP {(int)res.StatusCode}");
            }

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-8 Read response body", runId);
            var json = await res.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("[RUN {RunId}] STEP PV-UPU-8 BodyLength={Length}", runId, json?.Length ?? 0);

            _logger.LogInformation("[RUN {RunId}] STEP PV-UPU-9 Deserialize envelope", runId);
            var env = JsonSerializer.Deserialize<Envelope<UploadImage>>(json, JsonOpts);

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
}
