using Application.PixVerse;
using Application.PixVerse.Http;
using Application.Result;
using Infrastructure.PixVerse.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.PixVerse
{
    public class VideoClient(
        IPixVerseRequestHandler requestHandler,
        IVideoJobQueryClient videoJobQueryClient,
        ILogger<VideoClient> logger,
        IErrorHandler errorHandler
    ) : IVideoClient
    {
        private readonly IPixVerseRequestHandler _handler = requestHandler;
        private readonly IVideoJobQueryClient _videoJobQueryClient = videoJobQueryClient;
        private readonly ILogger<VideoClient> _logger = logger;
        private readonly IErrorHandler _error = errorHandler;

        public async Task<Operation<FileInfo>> DownloadAsync(
            long jobId,
            string destinationFilePath,
            int videoIndex = 0,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "Starting DownloadVideo jobId={JobId} videoIndex={VideoIndex} dest={Dest}",
                jobId, videoIndex, destinationFilePath);

            try
            {
                if (jobId == 0) return _error.Business<FileInfo>("jobId cannot be null or empty.");
                if (string.IsNullOrWhiteSpace(destinationFilePath)) return _error.Business<FileInfo>("destinationFilePath cannot be null or empty.");
                if (videoIndex < 0) return _error.Business<FileInfo>("videoIndex cannot be negative.");

                // 1) Get result -> video URL
                var resOp = await _videoJobQueryClient.GetResultAsync(jobId, ct);

                if (!resOp.IsSuccessful || resOp.Data is null)
                    return _error.Fail<FileInfo>(null, $"Cannot download video because generation result is not available. jobId={jobId}");

                var result = resOp.Data;
                if (result.VideoUrls is null || result.VideoUrls.Count == 0)
                    return _error.Fail<FileInfo>(null, $"No video URLs found for jobId={jobId}");

                if (videoIndex >= result.VideoUrls.Count)
                    return _error.Fail<FileInfo>(null, $"videoIndex out of range. videoIndex={videoIndex}, available={result.VideoUrls.Count}, jobId={jobId}");

                var videoUrl = result.VideoUrls[videoIndex];
                if (string.IsNullOrWhiteSpace(videoUrl))
                    return _error.Fail<FileInfo>(null, $"Video URL is empty for jobId={jobId}, videoIndex={videoIndex}");

                // 2) Resolve final path (client specific logic for filename)
                string finalPath;
                if (Directory.Exists(destinationFilePath) ||
                    destinationFilePath.EndsWith(Path.DirectorySeparatorChar) ||
                    destinationFilePath.EndsWith(Path.AltDirectorySeparatorChar))
                {
                    var fileName = $"{jobId}_{videoIndex}.mp4";
                    finalPath = Path.Combine(destinationFilePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), fileName);
                }
                else
                {
                    finalPath = destinationFilePath;
                    if (string.IsNullOrEmpty(Path.GetExtension(finalPath)))
                        finalPath += ".mp4";
                }

                // 3) Download using handler
                return await _handler.DownloadFileAsync(videoUrl, finalPath, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED PixVerse.DownloadVideo", jobId);
                return _error.Fail<FileInfo>(ex, $"PixVerse video download failed for jobId={jobId}");
            }
        }
    }
}
