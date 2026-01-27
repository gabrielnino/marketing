using Application.PixVerse;
using Application.PixVerse.Response;
using Application.Result.EnumType.Extensions;
using Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = AppHostBuilder.Create(args).Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var pixVerse = host.Services.GetRequiredService<IPixVerseService>();
        var CheckBalance = host.Services.GetRequiredService<ICheckBalance>();


        
        logger.LogInformation("=== START PixVerse LipSync TEST (GOKU IMAGE) ===");

        // -------------------------------------------------
        // 1) Check balance (optional but recommended)
        // -------------------------------------------------
        var balanceOp = await CheckBalance.CheckBalanceAsync();
        if (!balanceOp.IsSuccessful || balanceOp.Data is null)
        {
            logger.LogError("Balance check failed: {Message}", balanceOp.Message);
            return;
        }

        // -------------------------------------------------
        // 2) Upload Goku image (FILE)  <-- REQUIRED
        // -------------------------------------------------
        var gokuPath = @"E:\DocumentosCV\LuisNino\images\goku.jpg";

        if (!File.Exists(gokuPath))
        {
            logger.LogError("Goku image not found: {Path}", gokuPath);
            return;
        }

        //await using var fs = File.OpenRead(gokuPath);

        //var uploadImgOp = await pixVerse.UploadImageAsync(
        //    fs,
        //    Path.GetFileName(gokuPath),
        //    "image/jpeg");

        //if (!uploadImgOp.IsSuccessful || uploadImgOp.Data is null)
        //{
        //    logger.LogError("Goku image upload failed: {Message}", uploadImgOp.Message);
        //    return;
        //}

        //var imgId = uploadImgOp.Data.ImgId;
        //logger.LogInformation("Goku image uploaded. ImgId={ImgId}", imgId);

        //// -------------------------------------------------
        //// 3) Image-to-Video (creates source_video_id)
        //// -------------------------------------------------
        //var i2vReq = new ImageToVideoRequest
        //{
        //    ImgId = imgId,
        //    Duration = 5,
        //    Model = "v5",
        //    Quality = "540p",
        //    Prompt = "Goku speaking directly to camera, anime style, expressive mouth, clear face, neutral background",
        //    NegativePrompt = "blurry, distorted face, artifacts",
        //    Seed = 0
        //};

        //var i2vSubmitOp = await pixVerse.SubmitImageToVideoAsync(i2vReq);
        //if (!i2vSubmitOp.IsSuccessful || i2vSubmitOp.Data is null)
        //{
        //    logger.LogError("Image-to-Video failed: {Message}", i2vSubmitOp.Message);
        //    return;
        //}

        //var sourceVideoId = i2vSubmitOp.Data.JobId;
        //logger.LogInformation("Image-to-Video submitted. source_video_id={VideoId}", sourceVideoId);

        //var i2vOk = await PollUntilDoneAsync(
        //    logger, pixVerse, sourceVideoId,
        //    TimeSpan.FromMinutes(8),
        //    TimeSpan.FromSeconds(3),
        //    "I2V");

        //if (!i2vOk) return;

        //// -------------------------------------------------
        //// 4) LipSync (ONLY required capability)
        //// Case 2: source_video_id + TTS
        //// -------------------------------------------------
        //var lipReq = new LipSyncRequest
        //{
        //    SourceVideoId = sourceVideoId,
        //    LipSyncTtsSpeakerId = "auto",
        //    LipSyncTtsContent =
        //        "¡Hola Vancouver! Soy Goku. Siento un ki increíble aquí. "
        //      + "No olviden apoyar al Tricolor Fan Club. ¡Vamos con toda!"
        //};

        //var lipSubmitOp = await pixVerse.SubmitLipSyncAsync(lipReq);
        //if (!lipSubmitOp.IsSuccessful || lipSubmitOp.Data is null)
        //{
        //    logger.LogError("LipSync submit failed: {Message}", lipSubmitOp.Message);
        //    return;
        //}

        //var lipJobId = lipSubmitOp.Data.JobId;
        //logger.LogInformation("LipSync submitted. video_id={JobId}", lipJobId);

        //// -------------------------------------------------
        //// 5) Poll LipSync result
        //// -------------------------------------------------
        //var lipOk = await PollUntilDoneAsync(
        //    logger, pixVerse, lipJobId,
        //    TimeSpan.FromMinutes(10),
        //    TimeSpan.FromSeconds(3),
        //    "LIPSYNC");

        //if (!lipOk) return;

        await pixVerse.DownloadVideoAsync(383686258808174, @"E:\DocumentosCV\LuisNino\images\donwload\goku_lipsync_output.mp4");

        logger.LogInformation("=== END PixVerse LipSync TEST (GOKU IMAGE) ===");
    }

    private static async Task<bool> PollUntilDoneAsync(
        ILogger logger,
        IPixVerseService pixVerse,
        long jobId,
        TimeSpan maxWait,
        TimeSpan pollDelay,
        string label)
    {
        var deadline = DateTimeOffset.UtcNow + maxWait;

        while (true)
        {
            var op = await pixVerse.GetGenerationResultAsync(jobId);

            if (!op.IsSuccessful || op.Data is null)
            {
                logger.LogError("[{Label}] GetGenerationResult failed: {Message}", label, op.Message);
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
                logger.LogError("[{Label}] Timed out. JobId={JobId}", label, jobId);
                return false;
            }

            await Task.Delay(pollDelay);
        }
    }
}
