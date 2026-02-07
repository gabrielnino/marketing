using Application.PixVerse;
using Application.PixVerse.Http;
using Application.PixVerse.Response;
using Application.Result;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.PixVerse
{
    public class VideoJobQueryClient(
        IPixVerseRequestHandler requestHandler,
        ILogger<VideoJobQueryClient> logger
    ) : IVideoJobQueryClient
    {
        private readonly IPixVerseRequestHandler _handler = requestHandler;
        private readonly ILogger<VideoJobQueryClient> _logger = logger;

        public async Task<Operation<JobStatus>> GetStatusAsync(long jobId, CancellationToken ct = default)
        {
            var path = Api.StatusPath + Uri.EscapeDataString(jobId.ToString());
            _logger.LogInformation("Getting status for JobId={JobId}", jobId);
            return await _handler.GetAsync<JobStatus>(path, ct);
        }

        public async Task<Operation<JobResult>> GetResultAsync(long jobId, CancellationToken ct = default)
        {
            var path = Api.ResultPath + Uri.EscapeDataString(jobId.ToString());
            _logger.LogInformation("Getting result for JobId={JobId}", jobId);
            return await _handler.GetAsync<JobResult>(path, ct);
        }
    }
}
