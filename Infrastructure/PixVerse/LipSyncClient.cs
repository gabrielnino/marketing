using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Infrastructure.PixVerse
{
    public class LipSyncClient(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<ImageClient> logger
) : PixVerseBase(options.Value), ILipSyncClient
    {

        private readonly HttpClient _http = httpClient;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<ImageClient> _logger = logger;

        public async Task<Operation<JobSubmitted>> SubmitJobAsync(
       LipSync request,
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
                var env = JsonSerializer.Deserialize<Envelope<SubmitResp>>(json, JsonOpts);
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
}
