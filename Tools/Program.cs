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

namespace Tools
{
    public sealed class Program
    {
        private const string ConnectionMissingMessage =
            "Connection string 'DefaultConnection' is missing or empty.";

        private const string OutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var appConfig = new AppConfig();
            builder.Configuration.Bind(appConfig);
            builder.Services.AddSingleton(appConfig);
            builder.Host.UseSerilog((context, services, loggerConfig) =>
            {
                var logRoot = TryGetExecutionRunning(services) ?? context.HostingEnvironment.ContentRootPath;
                var logPath = Path.Combine(logRoot, "Logs");
                Directory.CreateDirectory(logPath);

                loggerConfig
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: Path.Combine(logPath, "Redirect-.log"),
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: 5_000_000,
                        retainedFileCountLimit: 7,
                        rollOnFileSizeLimit: true,
                        outputTemplate: OutputTemplate
                    );
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddMemoryCache();

            AddDbContextSQLite(builder);

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDataContext>(sp => sp.GetRequiredService<DataContext>());
            builder.Services.AddScoped<IErrorHandler, ErrorHandler>();
            builder.Services.AddScoped<IErrorLogCreate, ErrorLogCreate>();
            builder.Services.AddScoped<IErrorLogger, SerilogErrorLogger>();
            builder.Services.AddScoped<IErrorHandler, ErrorHandler>();
            builder.Services.AddSingleton<IColumnTypes, SQLite>();
            builder.Services.AddScoped<ITrackedLinkCreate, TrackedLinkCreate>();
            builder.Services.AddScoped<ITrackedLinkRead, TrackedLinkRead>();
            builder.Services.AddScoped<ITrackedLinkUpdate, TrackedLinkUpdate>();
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                if (!db.Initialize())
                {
                    throw new Exception("Database initialization failed");
                }
            }
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

        private static void AddDbContextSQLite(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), ConnectionMissingMessage);

            builder.Services.AddDbContext<DataContext>(options =>
            {
                options
                    .UseSqlite(connectionString, sqlite =>
                    {
                        var migrationsAssembly = typeof(DataContext).Assembly.GetName().Name;
                        sqlite.MigrationsAssembly(migrationsAssembly);
                    })
                    .AddInterceptors(new SqliteFunctionInterceptor());
            });
        }

        private static string? TryGetExecutionRunning(IServiceProvider services)
        {
            try
            {
                var tracker = services.GetService(typeof(ExecutionTracker)) as ExecutionTracker;
                return tracker?.ExecutionRunning;
            }
            catch
            {
                return null;
            }
        }
    }
}
