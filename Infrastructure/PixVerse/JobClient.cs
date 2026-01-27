using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.PixVerse
{
    public class JobClient(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<ImageClient> logger,
    IVideoJobQueryClient videoJobQueryClient
) : PixVerseBase(options.Value), IJobClient
    {
        private readonly PixVerseOptions _opt = options.Value;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<ImageClient> _logger = logger;
        private readonly IVideoJobQueryClient _videoJobQueryClient = videoJobQueryClient;

        public async Task<Operation<JobResult>> WaitForCompletionAsync(long jobId, CancellationToken ct = default)
        {
            var operation = "PixVerse.PixVerseBase.JobClient.WaitForCompletionAsync";
            var runId = NewRunId();
            _logger.LogInformation("[RUN {RunId}] START WaitForCompletion. JobId={JobId}", runId, jobId);

            if (jobId == 0)
            {
                _logger.LogWarning("[RUN {RunId}] WaitForCompletion aborted: jobId=0", runId);
                return _error.Business<JobResult>("jobId cannot be null or empty.");
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

                var st = await _videoJobQueryClient.GetStatusAsync(jobId, ct);

                if (!st.IsSuccessful)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-1 Status call failed. Poll={Poll}/{Max} JobId={JobId}", runId, i + 1, _opt.MaxPollingAttempts, jobId);
                    return st.ConvertTo<JobResult>();
                }

                if (st.Data is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-2 Invalid status payload (null). Poll={Poll}/{Max} JobId={JobId}", runId, i + 1, _opt.MaxPollingAttempts, jobId);
                    return _error.Fail<JobResult>(null, "Invalid status payload (null).");
                }

                _logger.LogInformation(
                    "[RUN {RunId}] STEP PV-POLL-3 Status received. State={State} IsTerminal={IsTerminal} Poll={Poll}/{Max} JobId={JobId}",
                    runId, st.Data.State, st.Data.IsTerminal, i + 1, _opt.MaxPollingAttempts, jobId);

                if (st.Data.IsTerminal)
                {
                    if (st.Data.State == JobState.Succeeded)
                    {
                        _logger.LogInformation("[RUN {RunId}] STEP PV-POLL-4 Terminal=Succeeded. Fetching result. JobId={JobId}", runId, jobId);
                        return await _videoJobQueryClient.GetResultAsync(jobId, ct);
                    }

                    var msg = $"Job ended with terminal state: {st.Data.State}.";
                    _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-4 Terminal!=Succeeded. {Message} JobId={JobId}", runId, msg, jobId);

                    return Operation<JobResult>.Success(new JobResult
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
            return _error.Fail<JobResult>(null, "Polling timed out.");
        }
    }
}
