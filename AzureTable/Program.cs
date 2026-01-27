using Application.PixVerse;
using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = AppHostBuilder.Create(args).Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        var imageClient = host.Services.GetRequiredService<IImageClient>();
        var balanceClient = host.Services.GetRequiredService<IBalanceClient>();
        var generationClient = host.Services.GetRequiredService<IGenerationClient>();
        var imageToVideoClient = host.Services.GetRequiredService<IImageToVideoClient>();
        var lipSyncClient = host.Services.GetRequiredService<ILipSyncClient>();

        logger.LogInformation("=== START PixVerse IMAGE->VIDEO + LIPSYNC(TTS) ===");

        // -------------------------------------------------
        // 1) Balance (optional)
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
        // 3) Submit Image->Video (creates a source video job)
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

        var sourceVideoId = i2vSubmitOp.Data.JobId; // IMPORTANT: this is the jobId to poll
        logger.LogInformation("Image-to-Video submitted. JobId(source_video_id)={JobId}", sourceVideoId);

        var i2vOk = await PollUntilDoneAsync(
            logger,
            generationClient,
            sourceVideoId,
            maxWait: TimeSpan.FromMinutes(10),
            pollDelay: TimeSpan.FromSeconds(3),
            label: "I2V");

        if (!i2vOk)
        {
            logger.LogError("Image-to-Video FAILED or TIMEOUT. JobId={JobId}", sourceVideoId);
            return;
        }


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
            maxWait: TimeSpan.FromMinutes(12),
            pollDelay: TimeSpan.FromSeconds(3),
            label: "LIPSYNC");

        if (!lipOk)
        {
            logger.LogError("LipSync FAILED or TIMEOUT. JobId={JobId}", lipJobId);
            return;
        }

        // -------------------------------------------------
        // 5) Download final MP4 (WITH VOICE)
        // -------------------------------------------------
        var outputPath = @"E:\DocumentosCV\LuisNino\images\donwload\final_with_voice.mp4";
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        await generationClient.DownloadVideoAsync(lipJobId, outputPath);

        logger.LogInformation("Final video downloaded: {Path}", outputPath);
        logger.LogInformation("=== END PixVerse IMAGE->VIDEO + LIPSYNC(TTS) ===");
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

            var result = op.Data;

            logger.LogInformation(
                "[{Label}] JobId={JobId} State={State}",
                label, jobId, result.State);

            if (result.State == JobState.Succeeded)
                return true;

            if (result.State == JobState.Failed)
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
