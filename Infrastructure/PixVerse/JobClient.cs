using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.PixVerse
{
    public class JobClient(
        IOptions<PixVerseOptions> options,
        IErrorHandler errorHandler,
        ILogger<JobClient> logger,
        IVideoJobQueryClient videoJobQueryClient
    ) : IJobClient
    {
        private readonly PixVerseOptions _opt = options.Value;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<JobClient> _logger = logger;
        private readonly IVideoJobQueryClient _videoJobQueryClient = videoJobQueryClient;

        public async Task<Operation<JobResult>> WaitForCompletionAsync(long jobId, CancellationToken ct = default)
        {
            if (jobId == 0) return _error.Business<JobResult>("jobId cannot be null or empty.");

            _logger.LogInformation("START WaitForCompletion. JobId={JobId} Attempts={Attempts} Interval={Interval}", jobId, _opt.MaxPollingAttempts, _opt.PollingInterval);

            for (var i = 0; i < _opt.MaxPollingAttempts; i++)
            {
                ct.ThrowIfCancellationRequested();

                _logger.LogDebug("Poll {Poll}/{Max}. JobId={JobId}", i + 1, _opt.MaxPollingAttempts, jobId);

                var st = await _videoJobQueryClient.GetStatusAsync(jobId, ct);

                if (!st.IsSuccessful)
                {
                     _logger.LogWarning("Status call failed. Poll={Poll}/{Max} JobId={JobId}", i + 1, _opt.MaxPollingAttempts, jobId);
                     return st.ConvertTo<JobResult>();
                }

                if (st.Data is null)
                {
                     _logger.LogWarning("Invalid status payload (null). Poll={Poll}/{Max} JobId={JobId}", i + 1, _opt.MaxPollingAttempts, jobId);
                     return _error.Fail<JobResult>(null, "Invalid status payload (null).");
                }

                if (st.Data.IsTerminal)
                {
                    if (st.Data.State == JobState.Succeeded)
                    {
                        _logger.LogInformation("Terminal=Succeeded. Fetching result. JobId={JobId}", jobId);
                        return await _videoJobQueryClient.GetResultAsync(jobId, ct);
                    }

                    var msg = $"Job ended with terminal state: {st.Data.State}.";
                    _logger.LogWarning("Terminal!=Succeeded. {Message} JobId={JobId}", msg, jobId);

                    return Operation<JobResult>.Success(new JobResult
                    {
                        RawJobId = jobId,
                        RawStatus = (int)st.Data.State
                    }, msg);
                }

                await Task.Delay(_opt.PollingInterval, ct);
            }

            _logger.LogError("FAILED WaitForCompletion: Polling timed out. JobId={JobId}", jobId);
            return _error.Fail<JobResult>(null, "Polling timed out.");
        }
    }
}
