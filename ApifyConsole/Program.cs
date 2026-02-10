using Application.Result;
using Configuration;
using Configuration.Apify;
using Configuration.Ats;
using Domain.OpenAI.Ats;
using Infrastructure.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services;
using Services.CV;
using Services.CV.Abstractions;
using Services.CV.Abstractions.OpenAI;
using Services.CV.Apify;
using Services.Interfaces;
using System.Text.Json;

namespace ApifyConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var client = host.Services.GetRequiredService<IApifyClient>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var errorHandler = host.Services.GetRequiredService<IErrorHandler>();

            try
            {
                var basePath = AppContext.BaseDirectory;
                var mappingPath = Path.Combine(basePath, "ErrorMappings.json");
                errorHandler.LoadErrorMappings(mappingPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load error mappings: {ex.Message}");
            }

            // Verify configuration
            var options = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<Configuration.Apify.ApifyOptions>>();
            if (string.IsNullOrEmpty(options.Value.ApiKey))
            {
                Console.WriteLine("ERROR: Apify ApiKey is missing or empty in configuration!");
            }
            else
            {
                Console.WriteLine("Configuration loaded successfully. ApiKey present.");
            }

            Console.WriteLine("Apify Console Client");
            Console.WriteLine("--------------------");

            while (true)
            {
                Console.WriteLine("\nAvailable commands:");
                Console.WriteLine("1. list-runs [limit] [offset] [desc:0/1]");
                Console.WriteLine("5. get-dataset-items");
                Console.WriteLine("6. exit");
                Console.Write("\nEnter command: ");

                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var command = parts[0].ToLower();

                try
                {
                    if (parts.Length < 3)
                    {
                        Console.WriteLine("Usage: score-offers <datasetId> <resumeFilePath> [limit]");
                        break;
                    }

                    var datasetId = parts[1];
                    var resumePath = parts[2];
                    var limit = parts.Length > 3 ? int.Parse(parts[3]) : 100;

                    if (!File.Exists(resumePath))
                    {
                        Console.WriteLine($"Error: Resume file not found at {resumePath}");
                        break;
                    }

                    var resumeText = await File.ReadAllTextAsync(resumePath);
                    Console.WriteLine($"Loaded resume ({resumeText.Length} chars). Fetching items...");

                    var itemsResult = await client.GetDatasetItemsAsync(datasetId, limit);
                    if (!itemsResult.IsSuccessful || itemsResult.Data == null)
                    {
                        Console.WriteLine($"Error fetching items: {itemsResult.Message}");
                        break;
                    }

                    Console.WriteLine($"Fetched {itemsResult.Data.Count} items. Scoring...");
                    //var atsService = host.Services.GetRequiredService<IAtsScoringService>();
                    //var scoresResult = await atsService.ScoreOffersAsync(resumeText, itemsResult.Data);

                    //if (scoresResult.IsSuccessful && scoresResult.Data != null)
                    //{
                    //    Console.WriteLine("--- Scoring Results ---");
                    //    foreach (var score in scoresResult.Data)
                    //    {
                    //        Console.WriteLine($"OfferId: {score.OfferId} | Score: {score.Score:F2}");
                    //    }
                    //}
                    //else
                    //{
                    //    Console.WriteLine($"Error scoring offers: {scoresResult.Message}");
                    //}

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var basePath = AppContext.BaseDirectory;
                    config.SetBasePath(basePath);
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole());

                    // Register dependencies manually since we are not using the full AppHostBuilder
                    services.AddSingleton<IErrorHandler, ErrorHandler>(); // Assuming simple ErrorHandler exists or we mock it?
                    // ErrorHandler usually depends on IErrorLogger. 
                    // Let's check if we can reuse AppHostBuilder or if we need to manually register minimal set.
                    // Infrastructure.Logging.ErrorHandler might have dependencies.
                    // Let's use a simpler fake error handler or register the real one if possible.
                    // Real one: public class ErrorHandler(IErrorLogger logger) : IErrorHandler
                    services.AddSingleton<IErrorLogger, ConsoleErrorLogger>(); // We need to implement this or use existing

                    services.Configure<ApifyOptions>(hostContext.Configuration.GetSection("Apify"));
                    services.AddHttpClient<IApifyClient, ApifyClient>();

                    services.Configure<AtsOptions>(hostContext.Configuration.GetSection("Ats"));

                    services.AddSingleton<IWebDriverFactory, ChromeDriverFactory>();

                    // OpenAI & Prompt Runner for Dynamic ATS Scoring
                    services.AddOptions<OpenAIConfig>()
                        .Bind(hostContext.Configuration.GetSection("WhatsApp:OpenAI"))
                        .ValidateOnStart();

                    services.AddHttpClient<IOpenAIClient, OpenAIClient>((sp, http) =>
                    {
                        // Basic configuration assuming OpenAIConfig is populated
                        var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAIConfig>>().Value;
                        if (string.IsNullOrWhiteSpace(opt.UriString)) return; // Skip if not configured? Or throw?

                        http.BaseAddress = new Uri(opt.UriString);
                        http.Timeout = TimeSpan.FromSeconds(60);

                        // Try machine env var first, then process, then direct val?
                        var apiKey = Environment.GetEnvironmentVariable(opt.ApiKey, EnvironmentVariableTarget.Machine)
                                     ?? Environment.GetEnvironmentVariable(opt.ApiKey)
                                     ?? opt.ApiKey; // Fallback if ApiKey is the key itself

                        if (!string.IsNullOrWhiteSpace(apiKey))
                        {
                            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                        }
                    });

                    services.AddSingleton<IProtectedTokensPromptLoader, ProtectedTokensPromptLoader>();
                    //services.AddSingleton<IProtectedTokensPromptRunner, ProtectedTokensPromptRunner>();

                    //services.AddTransient<IAtsScoringService, AtsScoringService>();
                });

        static async Task ListRuns(IApifyClient client, int limit, int offset, bool desc)
        {
            Console.WriteLine($"Fetching runs (limit={limit}, offset={offset}, desc={desc})...");
            var result = await client.GetRunsAsync(limit, offset, desc);
            if (result.IsSuccessful && result.Data != null)
            {
                Console.WriteLine($"Total: {result.Data.Total}, Count: {result.Data.Count}");
                foreach (var run in result.Data.Items)
                {
                    Console.WriteLine($"- {run.Id} | {run.Status} | Started: {run.StartedAt} | Act: {run.ActId}");
                }
            }
            else
            {
                Console.WriteLine($"Failed: {result.Message}");
            }
        }

        static async Task GetRun(IApifyClient client, string runId)
        {
            Console.WriteLine($"Fetching run {runId}...");
            var result = await client.GetRunAsync(runId);
            if (result.IsSuccessful && result.Data != null)
            {
                var run = result.Data;
                Console.WriteLine(JsonSerializer.Serialize(run, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine($"Failed: {result.Message}");
            }
        }

        static async Task GetResults(IApifyClient client, string runId)
        {
            Console.WriteLine($"Fetching results for run {runId}...");
            var result = await client.GetRunDatasetItemsAsync(runId);
            if (result.IsSuccessful && result.Data != null)
            {
                Console.WriteLine($"Items found: {result.Data.Count}");
                Console.WriteLine(JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine($"Failed: {result.Message}");
            }
        }
    }

    public class ConsoleErrorLogger : IErrorLogger
    {
        public Task LogAsync(Exception ex, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            return Task.CompletedTask;
        }
    }
}