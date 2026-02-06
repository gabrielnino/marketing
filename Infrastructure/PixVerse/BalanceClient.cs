using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.PixVerse
{
    public class BalanceClient(
        IPixVerseRequestHandler requestHandler,
        IOptions<PixVerseOptions> options,
        IErrorHandler errorHandler,
        ILogger<BalanceClient> logger
    ) : IBalanceClient
    {
        private readonly IPixVerseRequestHandler _handler = requestHandler;
        private readonly PixVerseOptions _opt = options.Value;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<BalanceClient> _logger = logger;

        public async Task<Operation<AccountCredits>> GetAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Starting PixVerse.CheckBalance");

            try
            {
                using var timeoutCts = _opt.HttpTimeout > TimeSpan.Zero
                   ? new CancellationTokenSource(_opt.HttpTimeout)
                   : new CancellationTokenSource();

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
                
                return await _handler.GetAsync<AccountCredits>(Api.BalancePath, linkedCts.Token);
            }
            catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "TIMEOUT CheckBalance after {Timeout}", _opt.HttpTimeout);
                return _error.Fail<AccountCredits>(ex, $"Balance check timed out after {_opt.HttpTimeout}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED CheckBalance");
                return _error.Fail<AccountCredits>(ex, "Balance check failed");
            }
        }
    }
}
