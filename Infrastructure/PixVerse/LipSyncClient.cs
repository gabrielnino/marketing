using Application.PixVerse;
using Application.PixVerse.Request;
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

namespace Infrastructure.PixVerse
{
    public class LipSyncClient(
        HttpClient httpClient,
        IOptions<PixVerseOptions> options,
        IErrorHandler errorHandler,
        ILogger<ImageClient> logger // se mantiene como está en tu código para no romper DI
    ) : PixVerseBase(options.Value), ILipSyncClient
    {
        private readonly HttpClient _http = httpClient;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<ImageClient> _logger = logger;

        // Ajusta si quieres más/menos texto en logs
        private const int MaxBodyLogChars = 4000;
        private const int MaxPayloadLogChars = 4000;

        public async Task<Operation<JobSubmitted>> SubmitJobAsync(
            LipSync request,
            CancellationToken ct = default)
        {
            var runId = NewRunId();
            _logger.LogInformation("[RUN {RunId}] START SubmitLipSync", runId);

            // Logs de entorno/cliente
            _logger.LogInformation(
                "[RUN {RunId}] Client settings: BaseAddress={BaseAddress} Timeout={TimeoutMs} DefaultHeaders={DefaultHeaders}",
                runId,
                _http.BaseAddress?.ToString() ?? "(null)",
                (int)_http.Timeout.TotalMilliseconds,
                DumpHeaders(_http.DefaultRequestHeaders)
            );

            try
            {
                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-1 Validate request", runId);

                // Log “before normalize” para ver qué llega realmente
                _logger.LogInformation(
                    "[RUN {RunId}] Request(before normalize): SourceVideoId={SourceVideoId} VideoMediaId={VideoMediaId} AudioMediaId={AudioMediaId} SpeakerId={SpeakerId} TtsLen={TtsLen}",
                    runId,
                    request.SourceVideoId,
                    request.VideoMediaId,
                    request.AudioMediaId,
                    request.LipSyncTtsSpeakerId,
                    request.LipSyncTtsContent?.Length ?? 0
                );

                request.Validate();
                request.Normalize();

                // Log “after normalize” para detectar cambios no esperados
                _logger.LogInformation(
                    "[RUN {RunId}] Request(after normalize): SourceVideoId={SourceVideoId} VideoMediaId={VideoMediaId} AudioMediaId={AudioMediaId} SpeakerId={SpeakerId} TtsLen={TtsLen}",
                    runId,
                    request.SourceVideoId,
                    request.VideoMediaId,
                    request.AudioMediaId,
                    request.LipSyncTtsSpeakerId,
                    request.LipSyncTtsContent?.Length ?? 0
                );

                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-2 Validate config", runId);
                if (!TryValidateConfig(out var configError))
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-LS-2 FAILED Config invalid: {Error}", runId, configError);
                    return _error.Fail<JobSubmitted>(null, configError);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-3 Build endpoint. Path={Path}", runId, Api.LipSyncPath);
                var endpoint = BuildEndpoint(Api.LipSyncPath);

                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-4 Serialize payload", runId);
                var payload = JsonSerializer.Serialize(request, JsonOpts);

                _logger.LogInformation(
                    "[RUN {RunId}] STEP PV-LS-4 PayloadLength={Length}",
                    runId,
                    payload?.Length ?? 0
                );

                // Log payload truncado para no reventar logs
                _logger.LogDebug(
                    "[RUN {RunId}] STEP PV-LS-4 Payload(truncated)={Payload}",
                    runId,
                    Truncate(payload, MaxPayloadLogChars)
                );

                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-5 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);

                using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                // Para asegurar content-type bien formado
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };

                // Antes de auth, log headers actuales
                _logger.LogDebug("[RUN {RunId}] STEP PV-LS-5 Request headers(before auth)={Headers}", runId, DumpHeaders(req.Headers));

                ApplyAuth(req);

                // Capturar trace-id real que se envía (PixVerseBase lo agrega si falta)
                var traceId = req.Headers.TryGetValues(Api.TraceIdHeader, out var traceVals)
                    ? traceVals.FirstOrDefault()
                    : null;

                _logger.LogInformation(
                    "[RUN {RunId}] STEP PV-LS-5 Auth applied. TraceId={TraceId} Headers(after auth)={Headers}",
                    runId,
                    traceId ?? "(null)",
                    DumpHeaders(req.Headers, redactApiKey: true)
                );

                // Content headers
                _logger.LogDebug(
                    "[RUN {RunId}] STEP PV-LS-5 Content headers={Headers}",
                    runId,
                    DumpHeaders(req.Content.Headers)
                );

                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-6 Send request", runId);

                HttpResponseMessage? res = null;
                var startedAt = DateTimeOffset.UtcNow;

                try
                {
                    res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
                }
                catch (TaskCanceledException tex) when (!ct.IsCancellationRequested)
                {
                    // Timeout de HttpClient típicamente llega como TaskCanceledException
                    _logger.LogError(
                        tex,
                        "[RUN {RunId}] STEP PV-LS-6 FAILED SendAsync TIMEOUT. ElapsedMs={ElapsedMs} Endpoint={Endpoint} TraceId={TraceId}",
                        runId,
                        (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds,
                        endpoint,
                        traceId ?? "(null)"
                    );
                    return _error.Fail<JobSubmitted>(tex, "SubmitLipSync failed (timeout).");
                }
                catch (OperationCanceledException ocex) when (ct.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        ocex,
                        "[RUN {RunId}] STEP PV-LS-6 CANCELED by caller. ElapsedMs={ElapsedMs} Endpoint={Endpoint} TraceId={TraceId}",
                        runId,
                        (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds,
                        endpoint,
                        traceId ?? "(null)"
                    );
                    return _error.Fail<JobSubmitted>(ocex, "SubmitLipSync canceled.");
                }

                _logger.LogInformation(
                    "[RUN {RunId}] STEP PV-LS-7 Response received. StatusCode={StatusCode} Reason={Reason} ElapsedMs={ElapsedMs}",
                    runId,
                    (int)res.StatusCode,
                    res.ReasonPhrase ?? "(null)",
                    (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds
                );

                _logger.LogDebug(
                    "[RUN {RunId}] STEP PV-LS-7 Response headers={Headers}",
                    runId,
                    DumpHeaders(res.Headers)
                );

                if (res.Content != null)
                {
                    _logger.LogDebug(
                        "[RUN {RunId}] STEP PV-LS-7 Response content headers={Headers}",
                        runId,
                        DumpHeaders(res.Content.Headers)
                    );
                }

                // Leer body SIEMPRE (también para errores), porque ahí viene la razón real del 400
                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-8 Read response body (always)", runId);
                var body = res.Content is null ? string.Empty : await res.Content.ReadAsStringAsync(ct);
                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-8 BodyLength={Length}", runId, body?.Length ?? 0);
                _logger.LogDebug("[RUN {RunId}] STEP PV-LS-8 Body(truncated)={Body}", runId, Truncate(body, MaxBodyLogChars));

                // Si NO es success, intentar parsear el body como Envelope para extraer ErrCode/ErrMsg si existe
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "[RUN {RunId}] STEP PV-LS-9 FAILED Non-success status. StatusCode={StatusCode} TraceId={TraceId}",
                        runId,
                        (int)res.StatusCode,
                        traceId ?? "(null)"
                    );

                    var envErr = TryDeserialize<Envelope<SubmitResp>>(body ?? string.Empty);
                    if (envErr is not null)
                    {
                        _logger.LogWarning(
                            "[RUN {RunId}] STEP PV-LS-9 Parsed error envelope. ErrCode={ErrCode} ErrMsg={ErrMsg}",
                            runId,
                            envErr.ErrCode,
                            envErr.ErrMsg
                        );
                    }
                    else
                    {
                        _logger.LogWarning("[RUN {RunId}] STEP PV-LS-9 Could not parse error envelope (raw body logged).", runId);
                    }

                    // Mensaje final con más contexto
                    var msg =
                        $"SubmitLipSync failed. HTTP {(int)res.StatusCode}. " +
                        $"Reason={res.ReasonPhrase}. TraceId={traceId}. " +
                        $"Body(truncated)={Truncate(body, 500)}";

                    return _error.Fail<JobSubmitted>(null, msg);
                }

                // Flujo success: deserializar envelope normal
                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-10 Deserialize envelope", runId);
                var env = JsonSerializer.Deserialize<Envelope<SubmitResp>>(body, JsonOpts);

                if (env is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-LS-10 FAILED Envelope is null", runId);
                    return _error.Fail<JobSubmitted>(null, "Invalid LipSync response (null envelope).");
                }

                _logger.LogInformation(
                    "[RUN {RunId}] STEP PV-LS-11 Validate envelope. ErrCode={ErrCode} ErrMsg={ErrMsg}",
                    runId,
                    env.ErrCode,
                    env.ErrMsg
                );

                if (env.ErrCode != 0)
                {
                    _logger.LogWarning(
                        "[RUN {RunId}] STEP PV-LS-11 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}",
                        runId,
                        env.ErrCode,
                        env.ErrMsg
                    );
                    return _error.Fail<JobSubmitted>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                }

                if (env.Resp is null || env.Resp.VideoId == 0)
                {
                    _logger.LogWarning(
                        "[RUN {RunId}] STEP PV-LS-11 FAILED Missing Resp.video_id. VideoId={VideoId}",
                        runId,
                        env.Resp?.VideoId ?? 0
                    );
                    return _error.Fail<JobSubmitted>(null, "Invalid LipSync response (missing Resp.video_id).");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-LS-12 Build result. VideoId={VideoId}", runId, env.Resp.VideoId);

                var submitted = new JobSubmitted
                {
                    JobId = env.Resp.VideoId,
                    Message = env.ErrMsg
                };

                _logger.LogInformation(
                    "[RUN {RunId}] SUCCESS SubmitLipSync. JobId={JobId} TraceId={TraceId}",
                    runId,
                    submitted.JobId,
                    traceId ?? "(null)"
                );

                return Operation<JobSubmitted>.Success(submitted, env.ErrMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] FAILED SubmitLipSync (exception)", runId);
                return _error.Fail<JobSubmitted>(ex, "SubmitLipSync failed");
            }
        }

        private static string Truncate(string? value, int maxChars)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Length <= maxChars) return value;
            return value.Substring(0, maxChars) + $"... (truncated, len={value.Length})";
        }

        private static string DumpHeaders(HttpHeaders headers, bool redactApiKey = false)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var h in headers)
                {
                    var key = h.Key;

                    if (redactApiKey && key.Equals(Api.ApiKeyHeader, StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append(key).Append("=").Append("[REDACTED]").Append("; ");
                        continue;
                    }

                    sb.Append(key).Append("=").Append(string.Join(",", h.Value)).Append("; ");
                }
                return sb.Length == 0 ? "(none)" : sb.ToString();
            }
            catch
            {
                return "(failed-to-dump-headers)";
            }
        }
    }
}
