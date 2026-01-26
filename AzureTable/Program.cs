using Application.PixVerse;
using Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Abstractions.OpenAI.news;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = AppHostBuilder.Create(args).Build();
        //var runner = host.Services.GetRequiredService<IJsonPromptRunner>();
        //await runner.RunStrictJsonAsync();

        ////var trackedLink = host.Services.GetRequiredService<ITrackedLink>();
        ////await trackedLink.UpsertAsync("google", "https://www.google.com/");

        //using var httpClient = new HttpClient();
        //httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

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
            "Balance OK. Credits={Credits}",
            balanceOp.Data);

        // -----------------------------
        // 2) Submit Text-to-Video
        // -----------------------------
        //var request = new PixVerseTextToVideoRequest
        //{
        //    AspectRatio = "16:9",
        //    Duration = 5,
        //    Model = "v5",
        //    NegativePrompt = "string",
        //    Prompt = "string",
        //    Quality = "540p",
        //    Seed = 0
        //};


        //var submitOp = await pixVerse.SubmitTextToVideoAsync(request);

        //if (!submitOp.IsSuccessful || submitOp.Data is null)
        //{
        //    logger.LogError("Submit failed: {Message}", submitOp.Message);
        //    return;
        //}

        //var jobId = submitOp.Data.JobId;

        //logger.LogInformation(
        //    "Job submitted successfully. JobId={JobId}",
        //    jobId.ToString());

        // -----------------------------
        // 3) Wait for completion
        // -----------------------------
        //var resultOp = await pixVerse.WaitForCompletionAsync(jobId.ToString());

        //if (!resultOp.IsSuccessful || resultOp.Data is null)
        //{
        //    logger.LogError("Generation failed: {Message}", resultOp.Message);
        //    return;
        //}

        //logger.LogInformation(
        //    "Generation completed successfully. VideoUrl={Url}",
        //    resultOp.Data);

        logger.LogInformation("=== END PixVerseService TEST ===");

    }
}