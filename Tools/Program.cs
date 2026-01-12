using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Application.WhatsApp.UseCases.Repository.CRUD;
using Configuration;
using Infrastructure.Repositories.CRUD;
using Infrastructure.Result;
using Infrastructure.WhatsApp.Repositories.CRUD;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Implementation;
using Persistence.Context.Interceptors;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Serilog;

namespace Api
{
    internal class Program
    {
        private const string Connection = "Connection string 'DefaultConnection' is missing or empty.";
        private const string AppSettingsFileName = "appsettings.json";
        private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}";

        private static void Main(string[] args)
        {
            var appConfig = new AppConfig();

            var basePath = Directory.GetCurrentDirectory();
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    Configure(config, basePath);
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    hostingContext.Configuration.Bind(appConfig);
                    services.AddSingleton(appConfig);
                    AddDbContextSQLite(hostingContext, services);
                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    services.AddScoped<IDataContext, DataContext>();
                    services.AddScoped<IDataContext>(sp => sp.GetRequiredService<DataContext>());
                    services.AddScoped<IErrorHandler, ErrorHandler>();
                    services.AddScoped<IErrorLogCreate, ErrorLogCreate>();
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

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

           
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
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

        private static void AddDbContextSQLite( HostBuilderContext context,IServiceCollection services)
        {



            var connectionString =  context.Configuration.GetConnectionString("DefaultConnection");

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