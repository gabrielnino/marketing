using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.PixVerse
{
    public class VideoJobQueryClient(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<ImageClient> logger
) : PixVerseBase(options.Value), IVideoJobQueryClient
    {
        private readonly HttpClient _http = httpClient;
        private readonly PixVerseOptions _opt = options.Value;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<ImageClient> _logger = logger;
        public async Task<Operation<JobStatus>> GetStatusAsync(long jobId, CancellationToken ct = default)
        {
            var runId = NewRunId();
            _logger.LogInformation("[RUN {RunId}] START GetGenerationStatus. JobId={JobId}", runId, jobId);

            try
            {
                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-1 Validate config", runId);
                if (!TryValidateConfig(out var configError))
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-ST-1 FAILED Config invalid: {Error}", runId, configError);
                    return _error.Fail<JobStatus>(null, configError);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-2 Build endpoint. JobId={JobId}", runId, jobId);
                var endpoint = BuildEndpoint(Api.StatusPath + Uri.EscapeDataString(jobId.ToString()));

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
                using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
                ApplyAuth(req);

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-4 Send request", runId);
                using var res = await _http.SendAsync(req, ct);

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-5 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-ST-5 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                    return _error.Fail<JobStatus>(null, $"Status failed. HTTP {(int)res.StatusCode}");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-6 Read response body", runId);
                var json = await res.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("[RUN {RunId}] STEP PV-ST-6 BodyLength={Length}", runId, json?.Length ?? 0);

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-7 Try deserialize envelope", runId);
                var env = TryDeserialize<Envelope<JobStatus>>(json);

                if (env is not null)
                {
                    _logger.LogInformation("[RUN {RunId}] STEP PV-ST-8 Envelope parsed. ErrCode={ErrCode}", runId, env.ErrCode);

                    if (env.ErrCode != 0)
                    {
                        _logger.LogWarning("[RUN {RunId}] STEP PV-ST-8 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                        return _error.Fail<JobStatus>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                    }

                    if (env.Resp is null)
                    {
                        _logger.LogWarning("[RUN {RunId}] STEP PV-ST-8 FAILED Resp is null", runId);
                        return _error.Fail<JobStatus>(null, "Invalid status payload (Resp null).");
                    }

                    _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationStatus (envelope)", runId);
                    return Operation<JobStatus>.Success(env.Resp, env.ErrMsg);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-9 Envelope parse failed. Fallback to raw status model", runId);
                var status = JsonSerializer.Deserialize<JobStatus>(json, JsonOpts);

                if (status is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-ST-9 FAILED Status model is null", runId);
                    return _error.Fail<JobStatus>(null, "Invalid status payload");
                }

                _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationStatus (raw)", runId);
                return Operation<JobStatus>.Success(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] FAILED GetGenerationStatus. JobId={JobId}", runId, jobId);
                return _error.Fail<JobStatus>(ex, "Status check failed");
            }
        }

        public async Task<Operation<JobResult>> GetResultAsync(long jobId, CancellationToken ct = default)
        {
            var runId = NewRunId();
            _logger.LogInformation("[RUN {RunId}] START GetGenerationResult. JobId={JobId}", runId, jobId);

            try
            {
                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-1 Validate config", runId);
                if (!TryValidateConfig(out var configError))
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-1 FAILED Config invalid: {Error}", runId, configError);
                    return _error.Fail<JobResult>(null, configError);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-2 Build endpoint. JobId={JobId}", runId, jobId);
                var endpoint = BuildEndpoint(Api.ResultPath + Uri.EscapeDataString(jobId.ToString()));

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
                using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
                ApplyAuth(req);

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-4 Send request", runId);
                using var res = await _http.SendAsync(req, ct);

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-5 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-5 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                    return _error.Fail<JobResult>(null, $"Result failed. HTTP {(int)res.StatusCode}");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-6 Read response body", runId);
                var json = await res.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("[RUN {RunId}] STEP PV-RS-6 BodyLength={Length}", runId, json?.Length ?? 0);

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-7 Try deserialize envelope", runId);
                var env = TryDeserialize<Envelope<JobResult>>(json);

                if (env is not null)
                {
                    _logger.LogInformation("[RUN {RunId}] STEP PV-RS-8 Envelope parsed. ErrCode={ErrCode}", runId, env.ErrCode);

                    if (env.ErrCode != 0)
                    {
                        _logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                        return _error.Fail<JobResult>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                    }

                    if (env.Resp is null)
                    {
                        _logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED Resp is null", runId);
                        return _error.Fail<JobResult>(null, "Invalid result payload (Resp null).");
                    }

                    _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (envelope)", runId);
                    return Operation<JobResult>.Success(env.Resp, env.ErrMsg);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-9 Envelope parse failed. Fallback to raw result model", runId);
                var result = JsonSerializer.Deserialize<JobResult>(json, JsonOpts);

                if (result is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-9 FAILED Result model is null", runId);
                    return _error.Fail<JobResult>(null, "Invalid result payload");
                }

                _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (raw)", runId);
                return Operation<JobResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] FAILED GetGenerationResult. JobId={JobId}", runId, jobId);
                return _error.Fail<JobResult>(ex, "Result check failed");
            }
        }
    }
}
