using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Redirect.Func.Functions
{
    public static class RedirectDefense
    {
        private sealed record Counter(long WindowStartUnixMinute, int Count);

        private static readonly ConcurrentDictionary<string, Counter> _ipCounters = new();

        public static bool IsCodeValid(string code, IConfiguration cfg)
        {
            var min = cfg.GetValue("RedirectDefense:CodeMinLength", 4);
            var max = cfg.GetValue("RedirectDefense:CodeMaxLength", 12);
            var pattern = cfg.GetValue("RedirectDefense:CodeRegex", "^[a-zA-Z0-9_-]+$");

            if (string.IsNullOrWhiteSpace(code)) return false;
            if (code.Length < min || code.Length > max) return false;
            return Regex.IsMatch(code, pattern, RegexOptions.CultureInvariant);
        }

        public static bool IsRateLimited(string ip, IConfiguration cfg)
        {
            var limit = cfg.GetValue("RedirectDefense:RateLimitPerIpPerMinute", 60);
            var nowMinute = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60;

            while (true)
            {
                var current = _ipCounters.GetOrAdd(ip, _ => new Counter(nowMinute, 0));

                if (current.WindowStartUnixMinute != nowMinute)
                {
                    // reset window
                    if (_ipCounters.TryUpdate(ip, new Counter(nowMinute, 1), current))
                        return false;

                    continue;
                }

                if (current.Count >= limit)
                    return true;

                if (_ipCounters.TryUpdate(ip, current with { Count = current.Count + 1 }, current))
                    return false;
            }
        }

        public static string GetClientIp(HttpRequestData req)
        {
            // Minimal: prefer X-Forwarded-For if present, else remote endpoint if available.
            if (req.Headers.TryGetValues("X-Forwarded-For", out var values))
            {
                var raw = values.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(raw))
                    return raw.Split(',')[0].Trim();
            }
            return "unknown";
        }
    }

}
