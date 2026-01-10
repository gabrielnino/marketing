using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Configuration;
using Services.WhatsApp.Abstractions.Login;

namespace Services.WhatsApp.WhatsApp
{
    public sealed class ScheduledMessenger(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<SchedulerOptions> options,
    ILogger<ScheduledMessenger> logger) : BackgroundService
    {
        private IServiceScopeFactory ScopeFactory { get; } = scopeFactory;
        private IOptionsMonitor<SchedulerOptions> Options { get; } = options;
        private ILogger<ScheduledMessenger> Logger { get; } = logger;

        private readonly SemaphoreSlim _gate = new(1, 1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("WhatsAppSchedulerHostedService started.");
            using var scope = ScopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<IWhatsAppMessage>();
            await sender.LoginAsync();
            while (!stoppingToken.IsCancellationRequested)
            {
                var opt = Options.CurrentValue;
                if (!opt.Enabled)
                {
                    Logger.LogInformation("Scheduler disabled. Sleeping 60s.");
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                    continue;
                }

                var tz = ResolveTimeZone(opt.TimeZoneId);
                var nowLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

                var next = FindNextOccurrence(nowLocal, tz, opt);
                if (next is null)
                {
                    // No schedules at all -> backoff
                    Logger.LogWarning("No schedule times found. Sleeping 10 minutes.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                    continue;
                }

                var delay = next.Value - nowLocal;
                if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

                Logger.LogInformation("Next scheduled run at {NextLocal} (in {Delay}).", next.Value, delay);

                await Task.Delay(delay, stoppingToken);

                // Evitar ejecuciones concurrentes
                if (!await _gate.WaitAsync(0, stoppingToken))
                {
                    Logger.LogWarning("Previous run still in progress. Skipping this occurrence.");
                    continue;
                }

                try
                {
                    Logger.LogInformation("Scheduled run started.");
                    await sender.SendMessageAsync();
                    Logger.LogInformation("Scheduled run finished.");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // shutdown
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Scheduled run failed.");
                }
                finally
                {
                    _gate.Release();
                }
            }

            Logger.LogInformation("WhatsAppSchedulerHostedService stopped.");
        }

        private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
        {
            // En Windows el ID suele ser distinto; si ya lo tienes validado, esto puede ser directo.
            // Aquí lo dejamos simple y explícito.
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        private static DateTimeOffset? FindNextOccurrence(
            DateTimeOffset nowLocal,
            TimeZoneInfo tz,
            SchedulerOptions opt)
        {
            // Busca en una ventana de 8 días (hoy + 7) para garantizar “siguiente”
            for (var dayOffset = 0; dayOffset <= 7; dayOffset++)
            {
                var day = nowLocal.Date.AddDays(dayOffset);
                var dowName = day.DayOfWeek.ToString(); // Monday..Sunday

                if (!opt.Weekly.TryGetValue(dowName, out var times) || times is null || times.Count == 0)
                    continue;

                foreach (var t in times
                         .Select(ParseTime)
                         .Where(x => x is not null)
                         .Select(x => x!.Value)
                         .OrderBy(x => x))
                {
                    var localUnspecified = new DateTime(
                        day.Year,
                        day.Month,
                        day.Day,
                        t.Hours,
                        t.Minutes,
                        0,
                        DateTimeKind.Unspecified
                        );

                    if (tz.IsInvalidTime(localUnspecified))
                        continue; // skip invalid times (DST gaps)
                    var offset = tz.GetUtcOffset(localUnspecified);
                    var candidateLocal = new DateTimeOffset(localUnspecified, offset);
                    if (candidateLocal > nowLocal)
                        return candidateLocal;
                }
            }

            return null;
        }

        private static (int Hours, int Minutes)? ParseTime(string value)
        {

            if (TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out var ts))
                return ((int)ts.TotalHours, ts.Minutes);

            return null;
        }
    }
}