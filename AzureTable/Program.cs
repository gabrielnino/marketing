using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AzureTable
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // -------------------------------------------------
            // LOG FILE SETUP
            // -------------------------------------------------
            var logDir = @"E:\Marketing-Logs\PixVerse";
            Directory.CreateDirectory(logDir);

            var logFile = Path.Combine(
                logDir,
                $"PixVerse_{DateTime.UtcNow:yyyyMMdd_HHmmss}.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    logFile,
                    rollingInterval: RollingInterval.Infinite,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                using var loggerFactory = new SerilogLoggerFactory(Log.Logger, dispose: false);

                using var host = AppHostBuilder
                    .Create(args)
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<ILoggerFactory>(loggerFactory);
                    })
                    .Build();

                var logger = host.Services.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("=== START PixVerse IMAGE->VIDEO + LIPSYNC(TTS) ===");
                logger.LogInformation("Log file: {LogFile}", logFile);

                var imageClient = host.Services.GetRequiredService<IImageClient>();
                var balanceClient = host.Services.GetRequiredService<IBalanceClient>();
                var generationClient = host.Services.GetRequiredService<IGenerationClient>();
                var imageToVideoClient = host.Services.GetRequiredService<IImageToVideoClient>();
                var lipSyncClient = host.Services.GetRequiredService<ILipSyncClient>();

                // -------------------------------------------------
                // 1) Balance
                // -------------------------------------------------
                var balanceOp = await balanceClient.GetAsync();
                if (!balanceOp.IsSuccessful || balanceOp.Data is null)
                {
                    logger.LogError("Balance check failed: {Message}", balanceOp.Message);
                    return;
                }

                logger.LogInformation("Balance OK. Credits={Credits}", balanceOp.Data.TotalCredits);

                // -------------------------------------------------
                // 2) Upload image
                // -------------------------------------------------
                var imagePath = @"E:\DocumentosCV\LuisNino\images\luisNino.jpg";
                if (!File.Exists(imagePath))
                {
                    logger.LogError("Image not found: {Path}", imagePath);
                    return;
                }

                await using var fs = File.OpenRead(imagePath);

                var uploadImgOp = await imageClient.UploadAsync(
                    fs,
                    Path.GetFileName(imagePath),
                    "image/jpeg");

                if (!uploadImgOp.IsSuccessful || uploadImgOp.Data is null)
                {
                    logger.LogError("Image upload failed: {Message}", uploadImgOp.Message);
                    return;
                }

                var imgId = uploadImgOp.Data.ImgId;
                logger.LogInformation("Image uploaded. ImgId={ImgId}", imgId);

                // -------------------------------------------------
                // 3) Image -> Video
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

                var i2vSubmitOp = await imageToVideoClient.SubmitAsync(i2vReq);
                if (!i2vSubmitOp.IsSuccessful || i2vSubmitOp.Data is null)
                {
                    logger.LogError("Image-to-Video submit failed: {Message}", i2vSubmitOp.Message);
                    return;
                }

                var sourceVideoId = i2vSubmitOp.Data.JobId;
                logger.LogInformation("Image-to-Video submitted. JobId={JobId}", sourceVideoId);

                var i2vOk = await PollUntilDoneAsync(
                    logger,
                    generationClient,
                    sourceVideoId,
                    TimeSpan.FromMinutes(10),
                    TimeSpan.FromSeconds(3),
                    "I2V");

                if (!i2vOk)
                    return;

                // -------------------------------------------------
                // 4) LipSync
                // -------------------------------------------------
                var lipReq = new LipSync
                {
                    SourceVideoId = sourceVideoId,
                    LipSyncTtsSpeakerId = "auto",
                    LipSyncTtsContent = "Hola Andres, sí vamos a ir a practicar para la prueba de carretera."
                };

                var lipSubmitOp = await lipSyncClient.SubmitJobAsync(lipReq);
                if (!lipSubmitOp.IsSuccessful || lipSubmitOp.Data is null)
                {
                    logger.LogError("LipSync submit failed: {Message}", lipSubmitOp.Message);
                    return;
                }

                var lipJobId = lipSubmitOp.Data.JobId;
                logger.LogInformation("LipSync submitted. JobId={JobId}", lipJobId);

                var lipOk = await PollUntilDoneAsync(
                    logger,
                    generationClient,
                    lipJobId,
                    TimeSpan.FromMinutes(12),
                    TimeSpan.FromSeconds(3),
                    "LIPSYNC");

                if (!lipOk)
                    return;

                // -------------------------------------------------
                // 5) Download
                // -------------------------------------------------
                var outputPath = @"E:\DocumentosCV\LuisNino\images\donwload\final_with_voice.mp4";
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                await generationClient.DownloadVideoAsync(lipJobId, outputPath);

                logger.LogInformation("Final video downloaded: {Path}", outputPath);
                logger.LogInformation("=== END PixVerse IMAGE->VIDEO + LIPSYNC(TTS) ===");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task<bool> PollUntilDoneAsync(
            ILogger logger,
            IGenerationClient generationClient,
            long jobId,
            TimeSpan maxWait,
            TimeSpan pollDelay,
            string label)
        {
            var deadline = DateTimeOffset.UtcNow + maxWait;

            while (true)
            {
                var op = await generationClient.GetResultAsync(jobId);

                if (!op.IsSuccessful || op.Data is null)
                {
                    logger.LogError("[{Label}] GetResultAsync failed: {Message}", label, op.Message);
                    return false;
                }

                logger.LogInformation(
                    "[{Label}] JobId={JobId} State={State}",
                    label, jobId, op.Data.State);

                if (op.Data.State == JobState.Succeeded)
                    return true;

                if (op.Data.State == JobState.Failed)
                    return false;

                if (DateTimeOffset.UtcNow >= deadline)
                {
                    logger.LogError("[{Label}] TIMEOUT. JobId={JobId}", label, jobId);
                    return false;
                }

                await Task.Delay(pollDelay);
            }
        }
    }
}
