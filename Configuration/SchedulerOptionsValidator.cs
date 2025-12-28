using System.Globalization;
using Microsoft.Extensions.Options;

namespace Configuration
{
    public sealed class SchedulerOptionsValidator : IValidateOptions<SchedulerOptions>
    {
        public ValidateOptionsResult Validate(string? name, SchedulerOptions options)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(options.TimeZoneId))
                errors.Add("Scheduler:TimeZoneId is required.");

            // Validate TimeZoneId exists on the host OS
            if (!string.IsNullOrWhiteSpace(options.TimeZoneId))
            {
                try { TimeZoneInfo.FindSystemTimeZoneById(options.TimeZoneId); }
                catch (TimeZoneNotFoundException)
                {
                    errors.Add($"Scheduler:TimeZoneId '{options.TimeZoneId}' not found on this OS.");
                }
                catch (InvalidTimeZoneException)
                {
                    errors.Add($"Scheduler:TimeZoneId '{options.TimeZoneId}' is invalid.");
                }
            }

            // Validate weekly time strings
            foreach (var (day, times) in options.Weekly)
            {
                if (times is null) continue;

                for (var i = 0; i < times.Count; i++)
                {
                    var value = times[i];
                    if (!TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out _))
                        errors.Add($"Scheduler:Weekly:{day}[{i}] value '{value}' must be HH:mm (24-hour).");
                }
            }

            return errors.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(errors);
        }
    }
}
