using Bootstrapper;
using Commands;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context.Implementation;
using Serilog;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Information("🚀 Application started at {StartTime}", DateTimeOffset.Now);

        try
        {
            using var host = AppHostBuilder.Create(args).Build();

            Log.Information("Initializing database...");
            EnsureDatabaseInitialized(host.Services);
            Log.Information("Database initialized successfully");

            var commandFactory = host.Services.GetRequiredService<CommandFactory>();
            var commands = commandFactory.CreateCommand().ToList();
            var jobArgs = host.Services.GetRequiredService<CommandArgs>();

            Log.Information("Discovered {CommandCount} command(s) to execute", commands.Count);

            foreach (var command in commands)
            {
                var commandName = command.GetType().Name;

                try
                {
                    Log.Information("▶ Executing command: {CommandName}", commandName);
                    await command.ExecuteAsync(jobArgs.Arguments);
                    Log.Information("✔ Command completed successfully: {CommandName}", commandName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "✖ Command execution failed: {CommandName}", commandName);
                    throw new AggregateException($"Command '{commandName}' failed", ex);
                }
            }

            Log.Information("✅ All commands executed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Application terminated due to an unrecoverable error");
            Environment.ExitCode = 1;
        }
        finally
        {
            Log.Information("🧹 Flushing logs and shutting down");
            await Log.CloseAndFlushAsync();
        }
    }

    static void EnsureDatabaseInitialized(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();

        if (!db.Initialize())
        {
            throw new Exception("Database initialization failed");
        }
    }
}
