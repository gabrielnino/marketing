using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Commands;
using Configuration;
using Infrastructure.Repositories.CRUD;
using Infrastructure.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Context.Implementation;
using Persistence.Context.Interceptors;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Serilog;

namespace Bootstrapper
{
    public static class AppHostBuilder
    {
        public static IHostBuilder Create(string[] args)
        {
            var appConfig = new AppConfig();

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    // Bind config first (so Paths.OutFolder exists before creating ExecutionTracker)
                    hostingContext.Configuration.Bind(appConfig);

                    services.AddSingleton(appConfig);

                    // ExecutionTracker is "per execution", singleton is OK (no EF dependencies)
                    var executionTracker = new ExecutionTracker(appConfig.Paths.OutFolder);
                    Directory.CreateDirectory(executionTracker.ExecutionFolder);
                    services.AddSingleton(executionTracker);

                    services.AddSingleton(new CommandArgs(args));
                    services.AddSingleton<CommandFactory>();
                    services.AddTransient<HelpCommand>();

                    AddDbContextSQLite(hostingContext, services);

                    // EF-related types should be scoped
                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    services.AddScoped<IDataContext, DataContext>();
                    services.AddScoped<IDataContext>(sp => sp.GetRequiredService<DataContext>());

                    // FIX #1: do NOT register these as Singleton because they depend (directly/indirectly) on EF scoped services
                    services.AddScoped<IErrorHandler, ErrorHandler>();
                    services.AddScoped<IErrorLogCreate, ErrorLogCreate>();

                    services.AddSingleton<IColumnTypes, SQLite>();
                })
                .UseSerilog((context, services, loggerConfig) =>
                {
                    var execution = services.GetRequiredService<ExecutionTracker>();
                    var logPath = Path.Combine(execution.ExecutionFolder, "Logs");
                    Directory.CreateDirectory(logPath);

                    loggerConfig.MinimumLevel.Debug()
                        .WriteTo.Console()
                        .WriteTo.File(
                            path: Path.Combine(logPath, "LiveNetwork-.log"),
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 5_000_000,
                            retainedFileCountLimit: 7,
                            rollOnFileSizeLimit: true,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"
                        );
                });
        }

        private static void AddDbContextSQLite(HostBuilderContext context, IServiceCollection services)
        {
            var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString),
                    "Connection string 'DefaultConnection' is missing or empty.");

            services.AddDbContext<DataContext>(options =>
            {
                options
                    .UseSqlite(connectionString, sqlite =>
                    {
                        sqlite.MigrationsAssembly(typeof(DataContext).Assembly.GetName().Name);
                    })
                    .AddInterceptors(new SqliteFunctionInterceptor());
            });

            services.AddMemoryCache();
        }
    }
}
