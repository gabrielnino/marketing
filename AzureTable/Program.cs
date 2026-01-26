using Application.PixVerse;
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

        logger.LogInformation("=== START PixVerseService TEST ===");

        // -----------------------------
        // 1) Check balance
        // -----------------------------
        var balanceOp = await pixVerse.CheckBalanceAsync();

        if (!balanceOp.IsSuccessful || balanceOp.Data is null)
        {
            logger.LogError("Balance check failed: {Message}", balanceOp.Message);
            return;
        }

        logger.LogInformation(
            "Balance OK. AccountId={AccountId} Monthly={Monthly} Package={Package} Total={Total}",
            balanceOp.Data.AccountId,
            balanceOp.Data.CreditMonthly,
            balanceOp.Data.CreditPackage,
            balanceOp.Data.TotalCredits);

        // -----------------------------
        // 2) Upload image (URL)
        // -----------------------------
        var imageUrl =
            "https://media.pixverse.ai/openapi%2Ff4c512d1-0110-4360-8515-d84d788ca8d1test_image_auto.jpg";

        var uploadOp = await pixVerse.UploadImageAsync(imageUrl);

        if (!uploadOp.IsSuccessful || uploadOp.Data is null)
        {
            logger.LogError("Upload (url) failed: {Message}", uploadOp.Message);
            return;
        }

        logger.LogInformation(
            "Upload (url) OK. ImgId={ImgId} ImgUrl={ImgUrl}",
            uploadOp.Data.ImgId,
            uploadOp.Data.ImgUrl);

        // -----------------------------
        // 3) Upload image (FILE) -> to get img_id
        // -----------------------------
        var localPath = @"E:\DocumentosCV\LuisNino\images\140704-james-rodriguez-10a.webp"; // <-- change this

        if (!File.Exists(localPath))
        {
            logger.LogError("File not found: {Path}", localPath);
            return;
        }

        var ext = Path.GetExtension(localPath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        if (contentType == "application/octet-stream")
        {
            logger.LogError("Unsupported image extension: {Ext}", ext);
            return;
        }

        await using var fs = File.OpenRead(localPath);

        var uploadFileOp = await pixVerse.UploadImageAsync(
            fs,
            Path.GetFileName(localPath),
            contentType);

        if (!uploadFileOp.IsSuccessful || uploadFileOp.Data is null)
        {
            logger.LogError("Upload (file) failed: {Message}", uploadFileOp.Message);
            return;
        }

        logger.LogInformation(
            "Upload (file) OK. ImgId={ImgId} ImgUrl={ImgUrl}",
            uploadFileOp.Data.ImgId,
            uploadFileOp.Data.ImgUrl);

   
        // -----------------------------
        // 6) Submit Image-to-Video using ImgId
        // -----------------------------
        var i2vReq = new PixVerseImageToVideoRequest
        {
            Duration = 5,
            ImgId = uploadFileOp.Data.ImgId, // <-- REQUIRED
            Model = "v5",
            Prompt =
                "James Rodríguez celebrating a goal in World Cup 2014, cinematic slow motion, stadium lights, nostalgic, high detail",
            Quality = "540p",
            NegativePrompt = "blurry, low quality, artifacts",
            Seed = 0
        };

        var i2vSubmitOp = await pixVerse.SubmitImageToVideoAsync(i2vReq);

        if (!i2vSubmitOp.IsSuccessful || i2vSubmitOp.Data is null)
        {
            logger.LogError("Image-to-Video submit failed: {Message}", i2vSubmitOp.Message);
            return;
        }

        var i2vJobId = i2vSubmitOp.Data.JobId.ToString();
        logger.LogInformation("Image-to-Video job submitted. JobId={JobId}", i2vJobId);

        // -----------------------------
        // 7) Poll GetGenerationResultAsync for Image-to-Video
        // -----------------------------
        var i2vOk = await PollUntilDoneAsync(
            logger,
            pixVerse,
            i2vJobId,
            maxWait: TimeSpan.FromMinutes(8),
            pollDelay: TimeSpan.FromSeconds(3),
            label: "I2V");

        if (!i2vOk)
            return;

        // -----------------------------
        // 8) Submit TEXT-to-VIDEO (normal generation)
        // -----------------------------
        var t2vReq = new PixVerseTextToVideoRequest
        {
            AspectRatio = "16:9",
            Duration = 5,
            Model = "v5",
            Prompt = "Cinematic night street in Vancouver, neon reflections on wet asphalt, slow camera move, ultra detailed",
            Quality = "540p",
            NegativePrompt = "blurry, low quality, artifacts, watermark, text",
            Seed = 0
        };

        var t2vSubmitOp = await pixVerse.SubmitTextToVideoAsync(t2vReq);

        if (!t2vSubmitOp.IsSuccessful || t2vSubmitOp.Data is null)
        {
            logger.LogError("Text-to-Video submit failed: {Message}", t2vSubmitOp.Message);
            return;
        }

        var t2vJobId = t2vSubmitOp.Data.JobId.ToString();
        logger.LogInformation("Text-to-Video job submitted. JobId={JobId}", t2vJobId);

        // -----------------------------
        // 9) Poll GetGenerationResultAsync for Text-to-Video
        // -----------------------------
        var t2vOk = await PollUntilDoneAsync(
            logger,
            pixVerse,
            t2vJobId,
            maxWait: TimeSpan.FromMinutes(8),
            pollDelay: TimeSpan.FromSeconds(3),
            label: "T2V");

        if (!t2vOk)
            return;

        logger.LogInformation("=== END PixVerseService TEST ===");
    }

    private static async Task<bool> PollUntilDoneAsync(
        ILogger logger,
        IPixVerseService pixVerse,
        string jobId,
        TimeSpan maxWait,
        TimeSpan pollDelay,
        string label)
    {
        var deadline = DateTimeOffset.UtcNow + maxWait;

        while (true)
        {
            var getOp = await pixVerse.GetGenerationResultAsync(jobId);

            if (!getOp.IsSuccessful || getOp.Data is null)
            {
                logger.LogError("[{Label}] GetGenerationResult failed: {Message}", label, getOp.Message);
                return false;
            }

            // Adjust these fields to match YOUR model (e.g., Status as string).
            var status = getOp.Data ?? null;
            if (status is null)
            {
                logger.LogError("[{Label}] FAILED. JobId={JobId} Result={Result}", label, jobId, getOp.Data);
                return false;
            }

            logger.LogInformation("[{Label}] Status={Status} Result={Result}", label, status, getOp.Data);
            //PixVerseGenerationResult

            //PixVerseJobState State



            if (status.State == PixVerseJobState.Succeeded)
            {
                logger.LogInformation("[{Label}] Completed successfully. JobId={JobId}", label, jobId);
                return true;
            }

            if (status.State != PixVerseJobState.Succeeded)
            {
                logger.LogError("[{Label}] FAILED. JobId={JobId} Result={Result}", label, jobId, getOp.Data);
                return false;
            }

            if (DateTimeOffset.UtcNow >= deadline)
            {
                logger.LogError("[{Label}] Timed out after {MaxWait}. JobId={JobId}", label, maxWait, jobId);
                return false;
            }

            await Task.Delay(pollDelay);
        }
    }
}
