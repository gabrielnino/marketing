using Application.Result;
using Commands;
using Configuration;
using Infrastructure.Result;
using Infrastructure.WhatsApp.Repositories.CRUD;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using Persistence.Context.Implementation;
using Persistence.Context.Interceptors;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Serilog;
using Services;
using Services.Interfaces;
using Services.WhatsApp.Abstractions.Login;
using Services.WhatsApp.Abstractions.OpenChat;
using Services.WhatsApp.Abstractions.Search;
using Services.WhatsApp.Login;
using Services.WhatsApp.OpenChat;
using Services.WhatsApp.Selector;
using Services.WhatsApp.WhatsApp;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Application.WhatsApp.UseCases.Repository.CRUD;

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
                    services.AddOptions<SchedulerOptions>()
                        .Bind(hostingContext.Configuration.GetSection(SchedulerOptions.SectionName))
                        .PostConfigure(o =>
                        {
                            SetScheduler(o);
                        })
                        .ValidateOnStart();

                    services.AddOptions<MessageConfig>()
                        .Bind(hostingContext.Configuration.GetSection("WhatsApp:Message"))
                        .Validate(o =>
                            !string.IsNullOrWhiteSpace(o.ImageDirectory) &&
                            !string.IsNullOrWhiteSpace(o.ImageFileName) &&
                            !string.IsNullOrWhiteSpace(o.Caption),
                            FailureMessage
                        );

                    hostingContext.Configuration.Bind(appConfig);

                    var executionMode =
                        hostingContext.Configuration.GetValue<ExecutionMode>("ExecutionMode");

                    if (executionMode == ExecutionMode.Scheduler)
                    {
                        services.AddHostedService<ScheduledMessenger>();
                    }

                    services.AddSingleton(appConfig);

                    var executionTracker = new ExecutionTracker(appConfig.Paths.OutFolder);
                    var cleanupReport = executionTracker.CleanupOrphanedRunningFolders();
                    LogCleanupReport(cleanupReport);

                    Directory.CreateDirectory(executionTracker.ExecutionRunning);
                    services.AddSingleton(executionTracker);

                    if (executionMode == ExecutionMode.Command)
                    {
                        services.AddSingleton(new CommandArgs(args));
                        services.AddSingleton<CommandFactory>();
                        services.AddTransient<WhatsAppCommand>();
                        services.AddTransient<HelpCommand>();
                        services.AddHostedService<WebDriverLifetimeService>();
                    }

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
                    services.AddScoped<IErrorHandler, ErrorHandler>();
                    services.AddSingleton<IColumnTypes, SQLite>();

                    services.AddSingleton<ITrackedLinkCreate, TrackedLinkCreate>();
                    services.AddSingleton<ITrackedLinkRead, TrackedLinkRead>();
                    services.AddSingleton<ITrackedLinkUpdate, TrackedLinkUpdate>();

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
                            outputTemplate:
                                OutputTemplate
                        );
                });
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
            // 🔴 HARD FAIL – explicit, early, deterministic


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

            //foreach (var deleted in report.DeletedRunningFolders)
            //{
            //    Log.Information("Deleted orphaned execution folder: {FolderPath}", deleted);
            //}

            //foreach (var failure in report.DeleteFailures)
            //{
            //    Log.Warning(
            //        failure.Exception,
            //        "Failed to delete orphaned execution folder: {FolderPath}",
            //        failure.Path
            //    );
            //}

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

        private static void AddDbContextSQLite(
            HostBuilderContext context,
            IServiceCollection services
        )
        {



            var connectionString =
                context.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), Connection);
            }


            services.AddHttpClient();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            //services.AddSwaggerGen();
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
