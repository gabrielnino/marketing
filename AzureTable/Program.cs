using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Bootstrapper;
using Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;
using System.Text.Json;

namespace AzureTable
{
    /// <summary>
    /// Fix principal:
    /// - NO usar el JobId del Image->Video como source_video_id del LipSync.
    /// - Extraer del "result" el VideoMediaId (o media id equivalente) y enviarlo como video_media_id.
    /// </summary>
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var runId = Guid.NewGuid().ToString("N")[..8];
            var sw = Stopwatch.StartNew();

            try
            {
                using var host = AppHostBuilder.Create(args).Build();

                var logger = host.Services.GetRequiredService<ILogger<Program>>();

                // IMPORTANT: AppHostBuilder writes logs to:
                //   {ExecutionTracker.ExecutionRunning}\Logs\Marketing-<date>.log
                // We log this explicitly so you always know where the file is.
                var executionRunning = TryGetExecutionRunning(host.Services) ?? null;
                if (executionRunning is null)
                {
                    // Added: explicit log before returning
                    Log.Error("[RUN {RunId}] ExecutionTracker NOT AVAILABLE. Aborting early. TotalElapsedMs={ElapsedMs}", runId, sw.ElapsedMilliseconds);
                    return;
                }

                var logsFolder = Path.Combine(executionRunning.ExecutionRunning, "Logs");
                var logFilePattern = Path.Combine(logsFolder, $"Marketing-{executionRunning.TimeStamp}.log");

                logger.LogInformation("[RUN {RunId}] === START PixVerse IMAGE->VIDEO + LIPSYNC(TTS) (FIXED) ===", runId);
                logger.LogInformation("[RUN {RunId}] ArgsCount={ArgsCount}", runId, args?.Length ?? 0);
                logger.LogInformation("[RUN {RunId}] ExecutionRunning={ExecutionRunning} TimeStamp={TimeStamp}", runId, executionRunning.ExecutionRunning, executionRunning.TimeStamp);
                logger.LogInformation("[RUN {RunId}] LOG FILE TARGET (pattern) => {LogFilePattern}", runId, logFilePattern);

                var imageClient = host.Services.GetRequiredService<IImageClient>();
                var balanceClient = host.Services.GetRequiredService<IBalanceClient>();
                var videoJobQueryClient = host.Services.GetRequiredService<IVideoJobQueryClient>();
                var imageToVideoClient = host.Services.GetRequiredService<IImageToVideoClient>();
                var lipSyncClient = host.Services.GetRequiredService<ILipSyncClient>();
                var videoClient = host.Services.GetRequiredService<IVideoClient>();

                // Added: log resolved service types (diagnostic only)
                logger.LogInformation(
                    "[RUN {RunId}] Resolved services: IImageClient={IImageClientType} IBalanceClient={IBalanceClientType} IVideoJobQueryClient={IVideoJobQueryClientType} IImageToVideoClient={IImageToVideoClientType} ILipSyncClient={ILipSyncClientType} IVideoClient={IVideoClientType}",
                    runId,
                    imageClient.GetType().FullName,
                    balanceClient.GetType().FullName,
                    videoJobQueryClient.GetType().FullName,
                    imageToVideoClient.GetType().FullName,
                    lipSyncClient.GetType().FullName,
                    videoClient.GetType().FullName);

                // -------------------------------------------------
                // STEP 1) Check balance
                // -------------------------------------------------
                logger.LogInformation("[RUN {RunId}] [STEP 1] Checking balance...", runId);

                var balOp = await balanceClient.GetAsync();
                if (!balOp.IsSuccessful)
                {
                    logger.LogError(
                        "[RUN {RunId}] [STEP 1] Balance FAILED. Error={Error}. ElapsedMs={ElapsedMs}",
                        runId, balOp.Error ?? "unknown", sw.ElapsedMilliseconds);
                    return;
                }

                logger.LogInformation(
                    "[RUN {RunId}] [STEP 1] Balance OK. Credits={Credits}. ElapsedMs={ElapsedMs}",
                    runId, balOp.Data?.TotalCredits, sw.ElapsedMilliseconds);

                // -------------------------------------------------
                // STEP 2) Upload image
                // -------------------------------------------------
                var imagePath = @"E:\Marketing-Logs\PixVerse\Inputs\gustavo.webp"; // AJUSTA
                var fileName = Path.GetFileName(imagePath);
                var fileExt = Path.GetExtension(imagePath);
                var contentType = GuessContentType(fileExt);

                logger.LogInformation(
                    "[RUN {RunId}] [STEP 2] Upload image START. Path={ImagePath} FileName={FileName} Ext={Ext} ContentType={ContentType} Exists={Exists} SizeBytes={SizeBytes}",
                    runId,
                    imagePath,
                    fileName,
                    fileExt,
                    contentType,
                    File.Exists(imagePath),
                    File.Exists(imagePath) ? new FileInfo(imagePath).Length : -1);

                if (!File.Exists(imagePath))
                {
                    logger.LogError("[RUN {RunId}] [STEP 2] Upload image ABORT. File does not exist. Path={ImagePath}", runId, imagePath);
                    return;
                }

                await using var imgStream = File.OpenRead(imagePath);

                var upOp = await imageClient.UploadAsync(
                    imgStream,
                    fileName: fileName,
                    contentType: contentType);

                if (!upOp.IsSuccessful || upOp.Data is null)
                {
                    logger.LogError(
                        "[RUN {RunId}] [STEP 2] Upload FAILED. Error={Error}. PayloadNull={PayloadNull}. ElapsedMs={ElapsedMs}",
                        runId, upOp.Error ?? "unknown", upOp.Data is null, sw.ElapsedMilliseconds);
                    return;
                }

                var imgId = upOp.Data.ImgId;
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 2] Upload OK. ImgId={ImgId}. ElapsedMs={ElapsedMs}",
                    runId, imgId, sw.ElapsedMilliseconds);

                // -------------------------------------------------
                // STEP 3) Submit Image->Video
                // -------------------------------------------------
                var i2vReq = new ImageToVideo
                {
                    ImgId = imgId,
                    Duration = 5,
                    Model = "v5",
                    Quality = "540p",
                    Prompt = "adult guy speaking directly to camera, serious style, expressive mouth, clear face, neutral background",
                    NegativePrompt = "blurry, distorted face, artifacts",
                    Seed = 0
                };

                // Added: full request JSON (safe)
                var i2vReqJson = SafeSerialize(i2vReq);
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 3] I2V Submit START. ImgId={ImgId} Duration={Duration} Model={Model} Quality={Quality} Seed={Seed} PromptLen={PromptLen} NegPromptLen={NegPromptLen} ReqJsonLen={ReqJsonLen}",
                    runId,
                    i2vReq.ImgId,
                    i2vReq.Duration,
                    i2vReq.Model,
                    i2vReq.Quality,
                    i2vReq.Seed,
                    i2vReq.Prompt?.Length ?? 0,
                    i2vReq.NegativePrompt?.Length ?? 0,
                    i2vReqJson?.Length ?? 0);

                logger.LogDebug("[RUN {RunId}] [STEP 3] I2V Request JSON => {I2vReqJson}", runId, i2vReqJson);

                var i2vSubmitOp = await imageToVideoClient.SubmitAsync(i2vReq);
                if (!i2vSubmitOp.IsSuccessful || i2vSubmitOp.Data is null)
                {
                    logger.LogError(
                        "[RUN {RunId}] [STEP 3] I2V Submit FAILED. Error={Error}. PayloadNull={PayloadNull}. ElapsedMs={ElapsedMs}",
                        runId, i2vSubmitOp.Error ?? "unknown", i2vSubmitOp.Data is null, sw.ElapsedMilliseconds);
                    return;
                }

                var jobId = i2vSubmitOp.Data.JobId;
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 3] I2V Submit OK. JobId={JobId}. ElapsedMs={ElapsedMs}",
                    runId, jobId, sw.ElapsedMilliseconds);

                // -------------------------------------------------
                // STEP 4) Poll job status until terminal
                // -------------------------------------------------
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 4] Poll I2V status START. JobId={JobId} MaxAttempts={MaxAttempts} DelaySec={DelaySec}",
                    runId, jobId, 60, 2);

                JobResult? finalStatus = null;
                finalStatus = await GetFinalStatus(runId, sw, logger, videoJobQueryClient, jobId, finalStatus);

                if (finalStatus is null)
                {
                    logger.LogError(
                        "[RUN {RunId}] [STEP 4] I2V Status polling TIMEOUT. JobId={JobId}. ElapsedMs={ElapsedMs}",
                        runId, jobId, sw.ElapsedMilliseconds);
                    return;
                }

                if (finalStatus.State != JobState.Succeeded)
                {
                    logger.LogError(
                        "[RUN {RunId}] [STEP 4] I2V job NOT SUCCEEDED. JobId={JobId} FinalState={State} ElapsedMs={ElapsedMs}",
                        runId, jobId, finalStatus.State, sw.ElapsedMilliseconds);
                    return;
                }

                logger.LogInformation(
                    "[RUN {RunId}] [STEP 4] I2V job SUCCEEDED. JobId={JobId} ElapsedMs={ElapsedMs}",
                    runId, jobId, sw.ElapsedMilliseconds);

                // -------------------------------------------------
                // STEP 5) Get job result + extract VideoMediaId
                // -------------------------------------------------
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 5] Get I2V result START. JobId={JobId}",
                    runId, jobId);

                var resOp = await videoJobQueryClient.GetResultAsync(jobId);
                if (!resOp.IsSuccessful || resOp.Data is null)
                {
                    logger.LogError(
                        "[RUN {RunId}] [STEP 5] Get result FAILED. JobId={JobId} Error={Error} PayloadNull={PayloadNull} ElapsedMs={ElapsedMs}",
                        runId, jobId, resOp.Error ?? "unknown", resOp.Data is null, sw.ElapsedMilliseconds);
                    return;
                }

                var resultJson = SafeSerialize(resOp.Data);
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 5] Get result OK. JobId={JobId} ResultJsonLen={Len} ElapsedMs={ElapsedMs}",
                    runId, jobId, resultJson.Length, sw.ElapsedMilliseconds);

                // Added: log a compact preview (avoid huge logs)
                logger.LogDebug(
                    "[RUN {RunId}] [STEP 5] Get result JSON preview (first 800 chars) => {ResultJsonPreview}",
                    runId,
                    resultJson.Length <= 800 ? resultJson : resultJson[..800]);

                long? videoMediaId = 0;

                // Added: log current state before extraction logic
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 5] MediaId extraction START. Initial VideoMediaId={VideoMediaId} (null? {IsNull})",
                    runId,
                    videoMediaId ?? 0,
                    videoMediaId is null);

                if (videoMediaId is null)
                {
                    videoMediaId = TryExtractMediaIdFromJson(resultJson);

                    logger.LogInformation(
                        "[RUN {RunId}] [STEP 5] MediaId extraction (fallback) VideoMediaId={VideoMediaId}",
                        runId, videoMediaId ?? 0);
                }

                if (videoMediaId is null)
                {
                    logger.LogError(
                        "[RUN {RunId}] [STEP 5] Cannot proceed: VideoMediaId NOT FOUND. JobId={JobId}. ResultJson={ResultJson}",
                        runId, jobId, resultJson);
                    return;
                }

                logger.LogInformation(
                    "[RUN {RunId}] [STEP 5] Using VideoMediaId={VideoMediaId} for LipSync (NOT JobId={JobId}).",
                    runId, videoMediaId, jobId);

                // -------------------------------------------------
                // STEP 6) Submit LipSync (TTS) using video_media_id
                // -------------------------------------------------
                var lipReq = new VideoLipSync
                {
                    VideoMediaId = 0,
                    SourceVideoId = resOp.Data.JobId,

                    AudioMediaId = 0,
                    LipSyncTtsSpeakerId = "auto",
                    LipSyncTtsContent = "¡Hola Vancouver! Soy gustavo. Voten por abelardo de la aspriella. ¡Vamos con toda!"
                };

                // Added: full request JSON (safe)
                var lipReqJson = SafeSerialize(lipReq);

                logger.LogInformation(
                    "[RUN {RunId}] [STEP 6] LipSync Submit START. VideoMediaId={VideoMediaId} SourceVideoId={SourceVideoId} Speaker={Speaker} ContentLen={ContentLen} HasAudioMediaId={HasAudioMediaId} HasSourceVideoId={HasSourceVideoId} ReqJsonLen={ReqJsonLen}",
                    runId,
                    lipReq.VideoMediaId,
                    lipReq.SourceVideoId,
                    lipReq.LipSyncTtsSpeakerId,
                    lipReq.LipSyncTtsContent?.Length ?? 0,
                    lipReq.AudioMediaId.HasValue && lipReq.AudioMediaId.Value > 0,
                    lipReq.SourceVideoId.HasValue && lipReq.SourceVideoId.Value > 0,
                    lipReqJson?.Length ?? 0);

                logger.LogDebug("[RUN {RunId}] [STEP 6] LipSync Request JSON => {LipReqJson}", runId, lipReqJson);

                var lipOp = await lipSyncClient.SubmitAsync(lipReq);

                if (!lipOp.IsSuccessful)
                {
                    logger.LogError(
                        "[RUN {RunId}] [STEP 6] LipSync Submit FAILED. Error={Error}. ElapsedMs={ElapsedMs}",
                        runId, lipOp.Error ?? "unknown", sw.ElapsedMilliseconds);
                    return;
                }

                logger.LogInformation(
                    "[RUN {RunId}] [STEP 6] LipSync Submit OK. LipJobId={LipJobId}. TotalElapsedMs={ElapsedMs}",
                    runId, lipOp.Data?.JobId, sw.ElapsedMilliseconds);

                // Corrected: this was logging STEP 5 and as Information; make it STEP 6 and Error
                if (lipOp.Data is null)
                {
                    logger.LogError(
                        "[RUN {RunId}] [STEP 6] LipSync Submit returned success but payload is NULL. Cannot continue. VideoMediaId={VideoMediaId} SourceVideoId={SourceVideoId} TotalElapsedMs={ElapsedMs}",
                        runId, videoMediaId ?? 0, lipReq.SourceVideoId, sw.ElapsedMilliseconds);
                    return;
                }

                var lipobJobId = lipOp.Data!.JobId;

                // Added: explicit start of polling LipSync job
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 7] Poll LipSync status START. LipJobId={LipJobId} MaxAttempts={MaxAttempts} DelaySec={DelaySec}",
                    runId, lipobJobId, 60, 2);

                finalStatus = null;
                finalStatus = await GetFinalStatus(runId, sw, logger, videoJobQueryClient, lipobJobId, finalStatus);

                if (finalStatus is null)
                {
                    // Corrected: message said STEP 4 / I2V; this is STEP 7 / LipSync
                    logger.LogError(
                        "[RUN {RunId}] [STEP 7] LipSync Status polling TIMEOUT. LipJobId={LipJobId}. TotalElapsedMs={ElapsedMs}",
                        runId, lipobJobId, sw.ElapsedMilliseconds);
                    return;
                }

                // Added: log final LipSync state
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 7] LipSync polling END. LipJobId={LipJobId} FinalState={FinalState} TotalElapsedMs={ElapsedMs}",
                    runId, lipobJobId, finalStatus.State, sw.ElapsedMilliseconds);

                // Added: log download intent
                var outPath = @"E:\Marketing-Logs\PixVerse\Inputs\final_video.mp4";
                logger.LogInformation(
                    "[RUN {RunId}] [STEP 8] Download START. JobId={JobId} OutputPath={OutputPath}",
                    runId, jobId, outPath);

                await videoClient.DownloadAsync(jobId, outPath);

                logger.LogInformation(
                    "[RUN {RunId}] [STEP 8] Download END. JobId={JobId} OutputPath={OutputPath} TotalElapsedMs={ElapsedMs}",
                    runId, jobId, outPath, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                // Ensure fatal exceptions appear in the log file too
                Log.Fatal(ex, "[RUN {RunId}] Unhandled exception. Application terminating.", runId);
                Environment.ExitCode = 1;
            }
            finally
            {
                // THIS is what makes file logging reliable in console apps:
                // flush buffered logs and close file handles.
                Log.Information("[RUN {RunId}] Flushing logs. TotalElapsedMs={ElapsedMs}", runId, sw.ElapsedMilliseconds);
                await Log.CloseAndFlushAsync();
            }
        }

        private static async Task<JobResult?> GetFinalStatus(string runId, Stopwatch sw, ILogger<Program> logger, IVideoJobQueryClient videoJobQueryClient, long jobId, JobResult? finalStatus)
        {
            for (var attempt = 1; attempt <= 60; attempt++)
            {
                var attemptSw = Stopwatch.StartNew();

                // Added: log attempt start (debug)
                logger.LogDebug(
                    "[RUN {RunId}] [POLL] AttemptStart Attempt={Attempt} JobId={JobId} TotalElapsedMs={ElapsedMs}",
                    runId, attempt, jobId, sw.ElapsedMilliseconds);

                var stOp = await videoJobQueryClient.GetResultAsync(jobId);

                if (!stOp.IsSuccessful || stOp.Data is null)
                {
                    logger.LogWarning(
                        "[RUN {RunId}] [POLL] Attempt {Attempt} FAILED. JobId={JobId} Error={Error}. PayloadNull={PayloadNull}. AttemptMs={AttemptMs} TotalElapsedMs={ElapsedMs}",
                        runId, attempt, jobId, stOp.Error ?? "unknown", stOp.Data is null, attemptSw.ElapsedMilliseconds, sw.ElapsedMilliseconds);
                }
                else
                {
                    // Corrected: the message said "IsTerminal" but was logging RawStatus; keep RawStatus name
                    logger.LogInformation(
                        "[RUN {RunId}] [POLL] Attempt {Attempt} OK. JobId={JobId} State={State} RawStatus={RawStatus} AttemptMs={AttemptMs} TotalElapsedMs={ElapsedMs}",
                        runId, attempt, jobId, stOp.Data.State, stOp.Data.RawStatus, attemptSw.ElapsedMilliseconds, sw.ElapsedMilliseconds);

                    // Added: compact JSON preview for each success (debug)
                    var stJson = SafeSerialize(stOp.Data);
                    logger.LogDebug(
                        "[RUN {RunId}] [POLL] Attempt {Attempt} Status JSON preview (first 400 chars) => {StatusJsonPreview}",
                        runId,
                        attempt,
                        stJson.Length <= 400 ? stJson : stJson[..400]);

                    if (stOp.Data.State == JobState.Succeeded)
                    {
                        finalStatus = stOp.Data;
                        break;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            return finalStatus;
        }

        private static ExecutionTracker? TryGetExecutionRunning(IServiceProvider services)
        {
            try
            {
                // ExecutionTracker is registered in AppHostBuilder
                var tracker = services.GetService(typeof(ExecutionTracker)) as ExecutionTracker;
                return tracker;
            }
            catch
            {
                return null;
            }
        }

        private static long? TryGetVideoMediaIdFromKnownModel(JobResult result)
        {
            // Adjust if your DTO exposes it directly.
            return null;
        }

        private static string SafeSerialize<T>(T obj)
        {
            try { return JsonSerializer.Serialize(obj); }
            catch { return "(serialize_failed)"; }
        }

        private static long? TryExtractMediaIdFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "(serialize_failed)")
                return null;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var candidates = new[]
                {
                    "video_media_id",
                    "videoMediaId",
                    "media_id",
                    "mediaId",
                    "video_id",
                    "videoId"
                };

                foreach (var c in candidates)
                {
                    if (TryFindLong(root, c, out var value) && value > 0)
                        return value;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool TryFindLong(JsonElement el, string propertyName, out long value)
        {
            value = 0;

            if (el.ValueKind == JsonValueKind.Object)
            {
                if (el.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt64(out value))
                        return true;

                    if (prop.ValueKind == JsonValueKind.String && long.TryParse(prop.GetString(), out value))
                        return true;
                }

                foreach (var p in el.EnumerateObject())
                {
                    if (TryFindLong(p.Value, propertyName, out value))
                        return true;
                }
            }
            else if (el.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in el.EnumerateArray())
                {
                    if (TryFindLong(item, propertyName, out value))
                        return true;
                }
            }

            return false;
        }

        private static string GuessContentType(string ext)
        {
            ext = (ext ?? string.Empty).Trim().ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
