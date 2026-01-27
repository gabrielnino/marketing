using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using File = System.IO.File;

namespace Infrastructure.PixVerse
{
    public class GetGenerationStatus(
    HttpClient httpClient,
    IOptions<PixVerseOptions> options,
    IErrorHandler errorHandler,
    ILogger<PixVerseService> logger
) : BaseVerseService(options.Value), IGetGenerationStatus
    {
        private readonly HttpClient _http = httpClient;
        private readonly PixVerseOptions _opt = options.Value;
        private readonly IErrorHandler _error = errorHandler;
        private readonly ILogger<PixVerseService> _logger = logger;
        public async Task<Operation<GenerationStatus>> GetGenerationStatusAsync(long jobId, CancellationToken ct = default)
        {
            var runId = VerseApiSupport.NewRunId();
            _logger.LogInformation("[RUN {RunId}] START GetGenerationStatus. JobId={JobId}", runId, jobId);

            try
            {
                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-1 Validate config", runId);
                if (!Helper.TryValidateConfig(out var configError))
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-ST-1 FAILED Config invalid: {Error}", runId, configError);
                    return _error.Fail<GenerationStatus>(null, configError);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-2 Build endpoint. JobId={JobId}", runId, jobId);
                var endpoint = Helper.BuildEndpoint(ApiConstants.StatusPath + Uri.EscapeDataString(jobId.ToString()));

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
                using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
                Helper.ApplyAuth(req);

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-4 Send request", runId);
                using var res = await _http.SendAsync(req, ct);

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-5 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-ST-5 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                    return _error.Fail<GenerationStatus>(null, $"Status failed. HTTP {(int)res.StatusCode}");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-6 Read response body", runId);
                var json = await res.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("[RUN {RunId}] STEP PV-ST-6 BodyLength={Length}", runId, json?.Length ?? 0);

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-7 Try deserialize envelope", runId);
                var env = Helper.TryDeserialize<ApiEnvelope<GenerationStatus>>(json);

                if (env is not null)
                {
                    _logger.LogInformation("[RUN {RunId}] STEP PV-ST-8 Envelope parsed. ErrCode={ErrCode}", runId, env.ErrCode);

                    if (env.ErrCode != 0)
                    {
                        _logger.LogWarning("[RUN {RunId}] STEP PV-ST-8 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                        return _error.Fail<GenerationStatus>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                    }

                    if (env.Resp is null)
                    {
                        _logger.LogWarning("[RUN {RunId}] STEP PV-ST-8 FAILED Resp is null", runId);
                        return _error.Fail<GenerationStatus>(null, "Invalid status payload (Resp null).");
                    }

                    _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationStatus (envelope)", runId);
                    return Operation<GenerationStatus>.Success(env.Resp, env.ErrMsg);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-ST-9 Envelope parse failed. Fallback to raw status model", runId);
                var status = JsonSerializer.Deserialize<GenerationStatus>(json, JsonOpts);

                if (status is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-ST-9 FAILED Status model is null", runId);
                    return _error.Fail<GenerationStatus>(null, "Invalid status payload");
                }

                _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationStatus (raw)", runId);
                return Operation<GenerationStatus>.Success(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] FAILED GetGenerationStatus. JobId={JobId}", runId, jobId);
                return _error.Fail<GenerationStatus>(ex, "Status check failed");
            }
        }

        // -------------------------------------------------
        // Result
        // -------------------------------------------------
        public async Task<Operation<Generation>> GetGenerationResultAsync(long jobId, CancellationToken ct = default)
        {
            var runId = VerseApiSupport.NewRunId();
            _logger.LogInformation("[RUN {RunId}] START GetGenerationResult. JobId={JobId}", runId, jobId);

            try
            {
                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-1 Validate config", runId);
                if (!Helper.TryValidateConfig(out var configError))
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-1 FAILED Config invalid: {Error}", runId, configError);
                    return _error.Fail<Generation>(null, configError);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-2 Build endpoint. JobId={JobId}", runId, jobId);
                var endpoint = Helper.BuildEndpoint(ApiConstants.ResultPath + Uri.EscapeDataString(jobId.ToString()));

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-3 Create request + apply auth. Endpoint={Endpoint}", runId, endpoint);
                using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
                Helper.ApplyAuth(req);

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-4 Send request", runId);
                using var res = await _http.SendAsync(req, ct);

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-5 Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-5 FAILED Non-success status. StatusCode={StatusCode}", runId, (int)res.StatusCode);
                    return _error.Fail<Generation>(null, $"Result failed. HTTP {(int)res.StatusCode}");
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-6 Read response body", runId);
                var json = await res.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("[RUN {RunId}] STEP PV-RS-6 BodyLength={Length}", runId, json?.Length ?? 0);

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-7 Try deserialize envelope", runId);
                var env = Helper.TryDeserialize<ApiEnvelope<Generation>>(json);

                if (env is not null)
                {
                    _logger.LogInformation("[RUN {RunId}] STEP PV-RS-8 Envelope parsed. ErrCode={ErrCode}", runId, env.ErrCode);

                    if (env.ErrCode != 0)
                    {
                        _logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED PixVerse error. ErrCode={ErrCode} ErrMsg={ErrMsg}", runId, env.ErrCode, env.ErrMsg);
                        return _error.Fail<Generation>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                    }

                    if (env.Resp is null)
                    {
                        _logger.LogWarning("[RUN {RunId}] STEP PV-RS-8 FAILED Resp is null", runId);
                        return _error.Fail<Generation>(null, "Invalid result payload (Resp null).");
                    }

                    _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (envelope)", runId);
                    return Operation<Generation>.Success(env.Resp, env.ErrMsg);
                }

                _logger.LogInformation("[RUN {RunId}] STEP PV-RS-9 Envelope parse failed. Fallback to raw result model", runId);
                var result = JsonSerializer.Deserialize<Generation>(json, JsonOpts);

                if (result is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-RS-9 FAILED Result model is null", runId);
                    return _error.Fail<Generation>(null, "Invalid result payload");
                }

                _logger.LogInformation("[RUN {RunId}] SUCCESS GetGenerationResult (raw)", runId);
                return Operation<Generation>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] FAILED GetGenerationResult. JobId={JobId}", runId, jobId);
                return _error.Fail<Generation>(ex, "Result check failed");
            }
        }

        public async Task<Operation<Generation>> WaitForCompletionAsync(long jobId, CancellationToken ct = default)
        {
            var runId = VerseApiSupport.NewRunId();
            _logger.LogInformation("[RUN {RunId}] START WaitForCompletion. JobId={JobId}", runId, jobId);

            if (jobId == 0)
            {
                _logger.LogWarning("[RUN {RunId}] WaitForCompletion aborted: jobId=0", runId);
                return _error.Business<Generation>("jobId cannot be null or empty.");
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

                var st = await GetGenerationStatusAsync(jobId, ct);

                if (!st.IsSuccessful)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-1 Status call failed. Poll={Poll}/{Max} JobId={JobId}", runId, i + 1, _opt.MaxPollingAttempts, jobId);
                    return st.ConvertTo<Generation>();
                }

                if (st.Data is null)
                {
                    _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-2 Invalid status payload (null). Poll={Poll}/{Max} JobId={JobId}", runId, i + 1, _opt.MaxPollingAttempts, jobId);
                    return _error.Fail<Generation>(null, "Invalid status payload (null).");
                }

                _logger.LogInformation(
                    "[RUN {RunId}] STEP PV-POLL-3 Status received. State={State} IsTerminal={IsTerminal} Poll={Poll}/{Max} JobId={JobId}",
                    runId, st.Data.State, st.Data.IsTerminal, i + 1, _opt.MaxPollingAttempts, jobId);

                if (st.Data.IsTerminal)
                {
                    if (st.Data.State == JobState.Succeeded)
                    {
                        _logger.LogInformation("[RUN {RunId}] STEP PV-POLL-4 Terminal=Succeeded. Fetching result. JobId={JobId}", runId, jobId);
                        return await GetGenerationResultAsync(jobId, ct);
                    }

                    var msg = $"Job ended with terminal state: {st.Data.State}.";
                    _logger.LogWarning("[RUN {RunId}] STEP PV-POLL-4 Terminal!=Succeeded. {Message} JobId={JobId}", runId, msg, jobId);

                    return Operation<Generation>.Success(new Generation
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
            return _error.Fail<Generation>(null, "Polling timed out.");
        }

        public async Task<Operation<FileInfo>> DownloadVideoAsync(
       long jobId,
       string destinationFilePath,
       int videoIndex = 0,
       CancellationToken ct = default)
        {
            var runId = VerseApiSupport.NewRunId();
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
                var resOp = await GetGenerationResultAsync(jobId, ct);

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
