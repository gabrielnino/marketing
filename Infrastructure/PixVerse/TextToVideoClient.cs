using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.Logging;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Infrastructure.PixVerse
{
    public class TextToVideoClient(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<ImageClient> logger
) : PixVerseBase(options.Value), ITextToVideoClient
    {
        private readonly HttpClient _http = httpClient;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<ImageClient> _logger = logger;

        public async Task<Operation<JobReceipt>> SubmitTAsync(
       TextToVideo request,
       CancellationToken ct = default)
        {
            var operation = "PixVerse.TextToVideoClient.SubmitTAsync";
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
                    return _error.Fail<JobReceipt>(null, configError);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-3 Build endpoint. Path={Path}", runId, Api.TextToVideoPath);
                var endpoint = BuildEndpoint(Api.TextToVideoPath);

                _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-4 Serialize payload", runId);
                var payload = JsonSerializer.Serialize(request, JsonOpts);
                _logger.LogDebug("[RUN {RunId}] STEP PV-T2V-4 PayloadLength={Length}", runId, payload?.Length ?? 0);

                _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
                ApiPayloadLogger.LogResponse(
                    _logger,
                    runId,
                    operation,
                    payload
                );
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
                    return _error.Fail<JobReceipt>(null, $"Submit failed. HTTP {(int)res.StatusCode}");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-8 Read response body", runId);
                var json = await res.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("[RUN {RunId}] STEP PV-T2V-8 BodyLength={Length}", runId, json?.Length ?? 0);

                _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-9 Deserialize envelope", runId);
                var env = JsonSerializer.Deserialize<Envelope<SubmitResp>>(json, JsonOpts);
                if (env is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-T2V-9 FAILED Envelope is null", runId);
                    return _error.Fail<JobReceipt>(null, "Invalid submit response (null).");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-10 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
                if (env.ErrCode != 0)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-T2V-10 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                    return _error.Fail<JobReceipt>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                }

                if (env.Resp is null || env.Resp.VideoId == 0)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-T2V-10 FAILED Missing Resp.video_id. VideoId={VideoId}", runId, env.Resp?.VideoId ?? 0);
                    return _error.Fail<JobReceipt>(null, "Invalid submit response (missing Resp.video_id).");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-T2V-11 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);
                var submitted = new JobReceipt
                {
                    JobId = env.Resp.VideoId,
                    Message = env.ErrMsg
                };

                _logger.LogInformation("[RUN {RunId}] SUCCESS SubmitTextToVideo. JobId={JobId}", runId, submitted.JobId);
                return Operation<JobReceipt>.Success(submitted, env.ErrMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitTextToVideo", runId);
                return _error.Fail<JobReceipt>(ex, "Submit failed");
            }
        }
    }
}
