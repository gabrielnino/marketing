using Application.PixVerse;
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
        var imageUrl = "https://media.pixverse.ai/openapi%2Ff4c512d1-0110-4360-8515-d84d788ca8d1test_image_auto.jpg";

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

        // Detect content-type from extension (basic)
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
        // 4) Submit Image-to-Video using ImgId
        // -----------------------------
        var i2vReq = new PixVerseImageToVideoRequest
        {
            Duration = 5,
            ImgId = uploadFileOp.Data.ImgId, // <-- REQUIRED
            Model = "v5",
            Prompt = "James Rodríguez celebrating a goal in World Cup 2014, cinematic slow motion, stadium lights, nostalgic, high detail",
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

        var jobId = i2vSubmitOp.Data.JobId.ToString();

        logger.LogInformation("Image-to-Video job submitted. JobId={JobId}", jobId);

        // -----------------------------
        // 5) Wait for completion + fetch result
        // -----------------------------
        var resultOp = await pixVerse.WaitForCompletionAsync(jobId);

        if (!resultOp.IsSuccessful || resultOp.Data is null)
        {
            logger.LogError("Image-to-Video generation failed: {Message}", resultOp.Message);
            return;
        }

        // Adapt these properties to your PixVerseGenerationResult model
        logger.LogInformation("Image-to-Video completed. Result={Result}", resultOp.Data);

        logger.LogInformation("=== END PixVerseService TEST ===");
    }
}
