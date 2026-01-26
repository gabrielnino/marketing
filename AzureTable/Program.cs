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


        var localPath = @"E:\DocumentosCV\LuisNino\images\140704-james-rodriguez-10a.webp"; // <-- change this

        if (!File.Exists(localPath))
        {
            logger.LogError("File not found: {Path}", localPath);
            return;
        }

        // IMPORTANT: contentType must match the file type
        // jpg/jpeg -> image/jpeg
        // png      -> image/png
        // webp     -> image/webp
        var contentType = "image/jpeg";

        await using var fs = File.OpenRead(localPath);

        var uploadFileOp = await pixVerse.UploadImageAsync(
            fs,
            Path.GetFileName(localPath),
            contentType,
            CancellationToken.None);

        if (!uploadFileOp.IsSuccessful || uploadFileOp.Data is null)
        {
            logger.LogError("Upload (file) failed: {Message}", uploadFileOp.Message);
            return;
        }


        // -----------------------------
        // 2b) Upload image (FILE) - optional example
        // -----------------------------
        // NOTE: Set a real local path to an image file < 20MB (png/webp/jpeg/jpg).
        //var localPath = @"C:\temp\test.jpg";
        //await using var fs = File.OpenRead(localPath);
        //var uploadFileOp = await pixVerse.UploadImageAsync(
        //    fs,
        //    Path.GetFileName(localPath),
        //    "image/jpeg");
        //
        //if (!uploadFileOp.IsSuccessful || uploadFileOp.Data is null)
        //{
        //    logger.LogError("Upload (file) failed: {Message}", uploadFileOp.Message);
        //    return;
        //}
        //
        //logger.LogInformation(
        //    "Upload (file) OK. ImgId={ImgId} ImgUrl={ImgUrl}",
        //    uploadFileOp.Data.ImgId,
        //    uploadFileOp.Data.ImgUrl);

        logger.LogInformation("=== END PixVerseService TEST ===");
    }
}
