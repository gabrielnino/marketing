using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.PixVerse
{
    public class BalanceClient(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<ImageClient> logger
) : PixVerseBase(options.Value), IBalanceClient
    {
       

        private readonly HttpClient _http = httpClient;
        private readonly PixVerseOptions _opt = options.Value;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<ImageClient> _logger = logger;


        public async Task<Operation<AccountCredits>> GetAsync(CancellationToken ct = default)
        {
            var operation = "PixVerse.PixVerseBase.BalanceClient.GetAsync";
            var runId = NewRunId();
            _logger.LogInformation("[RUN {RunId}] START PixVerse.CheckBalance", runId);

            try
            {
                _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-1 Validate config", runId);
                if (!TryValidateConfig(out var configError))
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-BAL-1 FAILED Config invalid: {Error}", runId, configError);
                    return _error.Fail<AccountCredits>(null, configError);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-2 Build endpoint. Path={Path}", runId, Api.BalancePath);
                var endpoint = BuildEndpoint(Api.BalancePath);

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
                    return _error.Fail<AccountCredits>(null, $"PixVerse balance failed. HTTP {(int)res.StatusCode}");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-7 Read response body", runId);
                var json = await res.Content.ReadAsStringAsync(linkedCts.Token);
                _logger.LogDebug("[RUN {RunId}] STEP PV-BAL-7 BodyLength={Length}", runId, json?.Length ?? 0);

                _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-8 Deserialize envelope", runId);
                var env = TryDeserialize<Envelope<AccountCredits>>(json);
                if (env is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-BAL-8 FAILED Envelope is null", runId);
                    return _error.Fail<AccountCredits>(null, "Invalid balance response (null).");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-BAL-9 Validate envelope. ErrCode={ErrCode}", runId, env.ErrCode);
                if (env.ErrCode != 0)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-BAL-9 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                    return _error.Fail<AccountCredits>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                }

                if (env.Resp is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-BAL-9 FAILED Resp is null", runId);
                    return _error.Fail<AccountCredits>(null, "Invalid balance payload (Resp null).");
                }

                _logger.LogInformation("[RUN {RunId}] SUCCESS PixVerse.CheckBalance", runId);
                return Operation<AccountCredits>.Success(env.Resp, env.ErrMsg);
            }
            catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "[RUN {RunId}] TIMEOUT CheckBalance after {Timeout}", runId, _opt.HttpTimeout);
                return _error.Fail<AccountCredits>(ex, $"Balance check timed out after {_opt.HttpTimeout}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] FAILED CheckBalance", runId);
                return _error.Fail<AccountCredits>(ex, "Balance check failed");
            }
        }

    }
}
