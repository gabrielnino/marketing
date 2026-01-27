using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using File = System.IO.File;

namespace Infrastructure.PixVerse
{
    public class VideoClient(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<ImageClient> logger,
    IVideoJobQueryClient videoJobQueryClient
) : PixVerseBase(options.Value), IVideoClient
    {
        private readonly HttpClient _http = httpClient;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<ImageClient> _logger = logger;
        private readonly IVideoJobQueryClient _videoJobQueryClient = videoJobQueryClient;


        public async Task<Operation<FileInfo>> DownloadAsync(
            long jobId,
            string destinationFilePath,
            int videoIndex = 0,
            CancellationToken ct = default)
        {
            var runId = NewRunId();
            _logger.LogInformation(
                "[RUN {RunId}] START PixVerse.DownloadVideo jobId={JobId} videoIndex={VideoIndex} dest={Dest}",
                runId, jobId, videoIndex, destinationFilePath);

            try
            {
                if (jobId == 0)
                    return _error.Business<FileInfo>("jobId cannot be null or empty.");

                if (string.IsNullOrWhiteSpace(destinationFilePath))
                    return _error.Business<FileInfo>("destinationFilePath cannot be null or empty.");

                if (videoIndex < 0)
                    return _error.Business<FileInfo>("videoIndex cannot be negative.");

                // 1) Get result -> video URL
                _logger.LogInformation("[RUN {RunId}] STEP 1: Fetch generation result", runId);
                var resOp = await _videoJobQueryClient.GetResultAsync(jobId, ct);

                if (!resOp.IsSuccessful|| resOp.Data is null)
                    return _error.Fail<FileInfo>(null, $"Cannot download video because generation result is not available. jobId={jobId}");

                var result = resOp.Data;

                if (result.VideoUrls is null || result.VideoUrls.Count == 0)
                    return _error.Fail<FileInfo>(null, $"No video URLs found for jobId={jobId}");

                if (videoIndex >= result.VideoUrls.Count)
                    return _error.Fail<FileInfo>(
                        null,
                        $"videoIndex out of range. videoIndex={videoIndex}, available={result.VideoUrls.Count}, jobId={jobId}");

                var videoUrl = result.VideoUrls[videoIndex];

                if (string.IsNullOrWhiteSpace(videoUrl))
                    return _error.Fail<FileInfo>(null, $"Video URL is empty for jobId={jobId}, videoIndex={videoIndex}");

                _logger.LogInformation("[RUN {RunId}] STEP 2: Selected videoUrl={VideoUrl}", runId, videoUrl);

                // 2) Resolve final path (allow passing a folder)
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

                var dir = Path.GetDirectoryName(finalPath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                var tmpPath = finalPath + ".download.tmp";

                // 3) Download streaming -> temp file
                _logger.LogInformation("[RUN {RunId}] STEP 3: Downloading to temp file tmp={Tmp}", runId, tmpPath);

                using (var req = new HttpRequestMessage(HttpMethod.Get, videoUrl))
                {
                    // Some CDNs are public; if PixVerse requires auth for asset URLs, apply here:
                    // ApplyAuth(req);

                    using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

                    if (!resp.IsSuccessStatusCode)
                        return _error.Fail<FileInfo>(
                            null,
                            $"PixVerse video download failed. HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");

                    await using var httpStream = await resp.Content.ReadAsStreamAsync(ct);
                    await using var fileStream = new FileStream(
                        tmpPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 81920,
                        useAsync: true);

                    await httpStream.CopyToAsync(fileStream, ct);
                    await fileStream.FlushAsync(ct);
                }

                // 4) Atomically move temp -> final (best-effort)
                _logger.LogInformation("[RUN {RunId}] STEP 4: Moving temp to final final={Final}", runId, finalPath);

                if (System.IO.File.Exists(finalPath))
                    File.Delete(finalPath);

                File.Move(tmpPath, finalPath);

                var fi = new FileInfo(finalPath);
                _logger.LogInformation(
                    "[RUN {RunId}] SUCCESS PixVerse.DownloadVideo saved={Path} bytes={Bytes}",
                    runId, fi.FullName, fi.Length);

                return Operation<FileInfo>.Success(fi);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[RUN {RunId}] CANCELED PixVerse.DownloadVideo", runId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] FAILED PixVerse.DownloadVideo", runId);

                // best-effort cleanup of tmp file
                try
                {
                    var tmp = destinationFilePath + ".download.tmp";
                    if (File.Exists(tmp))
                        File.Delete(tmp);
                }
                catch { /* ignore */ }

                return _error.Fail<FileInfo>(ex, $"PixVerse video download failed for jobId={jobId}");
            }
        }
    }
}
