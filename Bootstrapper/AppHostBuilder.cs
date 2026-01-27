using Application.PixVerse;
using Application.Result;
using Application.TrackedLinks;
using Commands;
using Configuration;
using Configuration.PixVerse;
using Configuration.UrlValidation;
using Configuration.YouTube;
using Infrastructure.AzureTables;
using Infrastructure.PixVerse;
using Infrastructure.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Persistence.Context.Implementation;
using Persistence.Context.Interceptors;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Serilog;
using Services.Abstractions.AutoIt;
using Services.Abstractions.Check;
using Services.Abstractions.Login;
using Services.Abstractions.OpenAI;
using Services.Abstractions.OpenAI.news;
using Services.Abstractions.OpenChat;
using Services.Abstractions.Search;
using Services.Abstractions.UrlValidation;
using Services.Abstractions.YouTube;                 // ✅ NEW        // ✅ NEW (YouTubeApiOptions)
using Services.AutoIt;
using Services.Check;
using Services.Login;
using Services.OpenAI;
using Services.OpenAI.news;
using Services.OpenChat;
using Services.Selector;
using Services.UrlValidation;
using Services.WhatsApp;
using Services.YouTube;                              // ✅ NEW (YouTubeService/Discoverer)
using System.Net.Http.Headers;

namespace Bootstrapper
{
    public static class AppHostBuilder
    {
        private const string AppSettingsFileName = "appsettings.json";
        private const string Connection = "Connection string 'DefaultConnection' is missing or empty.";
        private const string FailureMessage = "WhatsApp:Message configuration is incomplete";
        private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}";

        public static IHostBuilder Create(string[] args)
        {
            var appConfig = new AppConfig();
            var basePath = Directory.GetCurrentDirectory();
            var appSettingsPath = Path.Combine(basePath, AppSettingsFileName);

            if (!File.Exists(appSettingsPath))
            {
                var configMessage = $"Required configuration file '{AppSettingsFileName}' was not found.";
                Log.Warning(configMessage);
                throw new FileNotFoundException(configMessage, appSettingsPath);
            }

            Log.Information($"The configuration file '{AppSettingsFileName}' was found.");

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    Configure(config, basePath);
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    // -----------------------------
                    // Scheduler / WhatsApp / AzureTables / OpenAI
                    // -----------------------------
                    services.AddOptions<SchedulerOptions>()
                        .Bind(hostingContext.Configuration.GetSection(SchedulerOptions.SectionName))
                        .PostConfigure(SetScheduler)
                        .ValidateOnStart();

                    services.AddOptions<MessageConfig>()
                        .Bind(hostingContext.Configuration.GetSection("WhatsApp:Message"))
                        .Validate(o =>
                            !string.IsNullOrWhiteSpace(o.ImageDirectory) &&
                            !string.IsNullOrWhiteSpace(o.ImageFileName) &&
                            !string.IsNullOrWhiteSpace(o.Caption),
                            FailureMessage
                        );

                    services.AddOptions<AzureTablesConfig>()
                        .Bind(hostingContext.Configuration.GetSection("WhatsApp:AzureTables"))
                        .Validate(o => !string.IsNullOrWhiteSpace(o.ServiceSasUrl),
                            "WhatsApp:AzureTables:ServiceSasUrl is required.")
                        .ValidateOnStart();

                    services.AddScoped<ITrackedLink>(sp =>
                    {
                        var opt = sp.GetRequiredService<IOptions<AzureTablesConfig>>().Value;
                        return new TrackedLink(opt.ServiceSasUrl);
                    });

                    services.AddOptions<OpenAIConfig>()
                        .Bind(hostingContext.Configuration.GetSection("WhatsApp:OpenAI"))
                        .Validate(o =>
                            !string.IsNullOrWhiteSpace(o.ApiKey) &&
                            !string.IsNullOrWhiteSpace(o.UriString) &&
                            !string.IsNullOrWhiteSpace(o.Model),
                            "Configuration WhatsApp:OpenAI is required.")
                        .ValidateOnStart();

                    services.AddHttpClient<IOpenAIClient, OpenAIClient>((sp, http) =>
                    {
                        var opt = sp.GetRequiredService<IOptions<OpenAIConfig>>().Value;

                        http.BaseAddress = new Uri(opt.UriString);
                        http.Timeout = TimeSpan.FromSeconds(60);

                        var apiKey = Environment.GetEnvironmentVariable(opt.ApiKey, EnvironmentVariableTarget.Machine);
                        if (string.IsNullOrWhiteSpace(apiKey))
                            throw new InvalidOperationException($"OpenAI API key env var '{opt.ApiKey}' was not found at Machine scope.");

                        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    });

                    // -----------------------------
                    // Bind AppConfig and execution mode
                    // -----------------------------
                    hostingContext.Configuration.Bind(appConfig);

                    var executionMode = hostingContext.Configuration.GetValue<ExecutionMode>("ExecutionMode");
                    if (executionMode == ExecutionMode.Scheduler)
                        services.AddHostedService<ScheduledMessenger>();

                    services.AddSingleton(appConfig);

                    // -----------------------------
                    // Execution tracker
                    // -----------------------------
                    var executionTracker = new ExecutionTracker(appConfig.Paths.OutFolder);
                    var cleanupReport = executionTracker.CleanupOrphanedRunningFolders();
                    LogCleanupReport(cleanupReport);

                    Directory.CreateDirectory(executionTracker.ExecutionRunning);
                    services.AddSingleton(executionTracker);

                    // -----------------------------
                    // Command mode
                    // -----------------------------
                    if (executionMode == ExecutionMode.Command)
                    {
                        services.AddSingleton(new CommandArgs(args));
                        services.AddSingleton<CommandFactory>();
                        services.AddTransient<WhatsAppCommand>();
                        services.AddTransient<HelpCommand>();
                        services.AddHostedService<WebDriverLifetimeService>();
                    }

                    // -----------------------------
                    // Core services
                    // -----------------------------
                    services.AddScoped<IMessage, Message>();

                    services.AddScoped<IWebDriver>(sp =>
                    {
                        var factory = sp.GetRequiredService<IWebDriverFactory>();
                        return factory.Create(false);
                    });

                    services.AddSingleton<ISecurityCheck, SecurityCheck>();
                    services.AddTransient<ILoginService, LoginService>();
                    services.AddTransient<ICaptureSnapshot, CaptureSnapshot>();
                    services.AddSingleton<IWebDriverFactory, ChromeDriverFactory>();
                    services.AddSingleton<IDirectoryCheck, DirectoryCheck>();
                    services.AddTransient<IOpenChat, OpenChat>();
                    services.AddTransient<IChatService, ChatService>();
                    services.AddTransient<IAttachments, Attachments>();
                    services.AddTransient<IAutoItRunner, AutoItRunner>();

                    AddDbContextSQLite(hostingContext, services);

                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    services.AddScoped<IDataContext, DataContext>();
                    services.AddScoped<IDataContext>(sp => sp.GetRequiredService<DataContext>());
                    services.AddSingleton<IErrorLogger, SerilogErrorLogger>();
                    services.AddScoped<IErrorHandler, ErrorHandler>();
                    services.AddSingleton<IColumnTypes, SQLite>();

                    // -----------------------------
                    // News / Prompt runner
                    // -----------------------------
                    services.AddSingleton<INewsHistoryStore, NewsHistoryStore>();
                    services.AddSingleton<INostalgiaPromptLoader, NostalgiaPromptLoader>();

                    // JsonPromptRunner now depends on IYouTubeService and options (you changed it)
                    services.AddSingleton<IJsonPromptRunner, JsonPromptRunner>();

                    // -----------------------------
                    // URL validation
                    // -----------------------------
                    services.AddUrlValidation(hostingContext.Configuration);

                    services.AddSingleton<ImageClient, ImageClient>();
                    services.AddSingleton<IBalanceClient, BalanceClient>();
                    services.AddSingleton<ImageClient, ImageClient>();
                    services.AddSingleton<IImageToVideoClient, ImageToVideoClient>();
                    services.AddSingleton<IGenerationClient, GenerationClient>();
                    services.AddSingleton<ILipSyncClient, LipSyncClient>();
                    services.AddSingleton<IImageClient, ImageClient>();
                    
                    // -----------------------------
                    // ✅ NEW: YouTube API + viral discoverer
                    // -----------------------------

                    // Bind YouTube API options (BaseUrl + ApiKey)
                    services.AddOptions<YouTubeApiOptions>()
                        .Bind(hostingContext.Configuration.GetSection(YouTubeApiOptions.SectionName))
                        .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "YouTube:ApiKey is required.")
                        .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "YouTube:BaseUrl is required.")
                        .ValidateOnStart();


        
                    // Typed HttpClient for YouTubeService using BaseUrl + API key is read inside the service
                    services.AddHttpClient<IYouTubeService, YouTubeService>((sp, http) =>
                    {
                        var opt = sp.GetRequiredService<IOptions<YouTubeApiOptions>>().Value;

                        http.BaseAddress = new Uri(opt.BaseUrl);
                        //http.Timeout = TimeSpan.FromSeconds(opt.HttpTimeoutSeconds);

                        http.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                    });

                    // Composite discoverer (if you are using it)
                    services.AddSingleton<IYouTubeViralVideoDiscoverer, YouTubeViralVideoDiscoverer>();

                    // If JsonPromptRunner uses extra runner options for query/options, bind them here:
                    // services.AddOptions<YouTubeCurationRunnerOptions>()
                    //     .Bind(hostingContext.Configuration.GetSection(YouTubeCurationRunnerOptions.SectionName))
                    //     .ValidateOnStart();

                    // -----------------------------
                    // Platform resolver (if still needed elsewhere; avoid double registration)
                    // -----------------------------
                    services.AddSingleton<IPlatformResolver, PlatformResolver>();
                    services.AddOptions<PixVerseOptions>()
                    .Bind(hostingContext.Configuration.GetSection("PixVerse"))
                    .ValidateOnStart();

                    //services.AddHttpClient<IPixVerseService, PixVerseService>();
                })
                .UseSerilog((context, services, loggerConfig) =>
                {
                    var execution = services.GetRequiredService<ExecutionTracker>();
                    var logPath = Path.Combine(execution.ExecutionRunning, "Logs");
                    Directory.CreateDirectory(logPath);

                    loggerConfig
                        .MinimumLevel.Debug()
                        .WriteTo.Console()
                        .WriteTo.File(
                            path: Path.Combine(logPath, "Marketing-.log"),
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 5_000_000,
                            retainedFileCountLimit: 7,
                            rollOnFileSizeLimit: true,
                            outputTemplate: OutputTemplate
                        );
                });
        }

        public static IServiceCollection AddUrlValidation(this IServiceCollection services, IConfiguration cfg)
        {
            services.Configure<UrlOptions>(cfg.GetSection(UrlOptions.SectionName));

            services.AddSingleton<IPlatformResolver, PlatformResolver>();
            services.AddSingleton<IUrlFactory, UrlValidatorFactory>();
            services.AddSingleton<IValidationPipeline, UrlValidationPipeline>();

            // Separate HttpClient per platform if you want different headers/timeouts later.
            services.AddHttpClient<YouTubeUrlValidator>();
            services.AddHttpClient<TikTokUrlValidator>();
            services.AddHttpClient<InstagramUrlValidator>();

            // Register all validators (multi-implementation)
            services.AddSingleton<IUrValidator>(sp => sp.GetRequiredService<YouTubeUrlValidator>());
            services.AddSingleton<IUrValidator>(sp => sp.GetRequiredService<TikTokUrlValidator>());
            services.AddSingleton<IUrValidator>(sp => sp.GetRequiredService<InstagramUrlValidator>());

            return services;
        }

        private static void SetScheduler(SchedulerOptions o)
        {
            foreach (var key in o.Weekly.Keys.ToList())
            {
                var list = o.Weekly[key] ?? [];
                List<string> value =
                [
                    .. list
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                ];
                o.Weekly[key] = value;
            }
        }

        private static void Configure(IConfigurationBuilder config, string basePath)
        {
            config.SetBasePath(basePath);

            config.AddJsonFile(
                path: AppSettingsFileName,
                optional: false,
                reloadOnChange: true
            );

            config.AddEnvironmentVariables();
        }

        public static void LogCleanupReport(CleanupReport report)
        {
            if (report is null)
            {
                Log.Warning("The folder is clean");
                return;
            }

            if (report.IsClean)
            {
                Log.Information("Execution cleanup completed with no errors");
            }
            else
            {
                Log.Warning(
                    "Execution cleanup completed with {FailureCount} failure(s)",
                    report.DeleteFailures.Count
                );
            }
        }

        private static void AddDbContextSQLite(HostBuilderContext context, IServiceCollection services)
        {
            var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(context), Connection);

            services.AddHttpClient();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddDistributedMemoryCache();

            services.AddDbContext<DataContext>(options =>
            {
                options
                    .UseSqlite(connectionString, sqlite =>
                    {
                        string? name = typeof(DataContext).Assembly.GetName().Name;
                        sqlite.MigrationsAssembly(name);
                    })
                    .AddInterceptors(new SqliteFunctionInterceptor());
            });

            services.AddMemoryCache();
        }
    }
}
