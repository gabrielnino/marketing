using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;
using System.Net;

namespace Services.UrlValidation
{
    public abstract class HttpBodyRuleValidatorBase(HttpClient httpClient, IOptionsMonitor<UrlValidationOptions> options) : IUrlAvailabilityValidator
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IOptionsMonitor<UrlValidationOptions> _options = options;

        public abstract UrlPlatform Platform { get; }

        protected abstract PlatformRules Rules(UrlValidationOptions opt);

        public async Task<UrlValidationResult> ValidateAsync(string url, CancellationToken ct = default)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return new UrlValidationResult(false, Platform, null, "Invalid absolute URL.");
            }
                

            var opt = _options.CurrentValue;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, opt.TimeoutSeconds)));

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                // Some platforms behave differently without a UA.
                req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120 Safari/537.36");

                using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                var status = (int)resp.StatusCode;

                // If hard HTTP fail (403/404 etc.) you can fail fast.
                if ((int)resp.StatusCode >= 400 && resp.StatusCode != HttpStatusCode.Forbidden)
                {
                    return new UrlValidationResult(false, Platform, status, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");
                }

                var body = await resp.Content.ReadAsStringAsync(cts.Token);
                if (body.Length > opt.MaxBodyCharsToScan)
                    body = body[..opt.MaxBodyCharsToScan];

                var rules = Rules(opt);

                // MustContain: all must be present
                foreach (var must in rules.MustContain)
                {
                    if (!ContainsIgnoreCase(body, must))
                    {
                        return new UrlValidationResult(
                            false, Platform, status,
                            $"Missing required marker: '{must}'.",
                            EvidenceSnippet: TakeSnippet(body, must)
                        );
                    }
                }

                // MustNotContain: none must be present
                foreach (var mustNot in rules.MustNotContain)
                {
                    if (ContainsIgnoreCase(body, mustNot))
                    {
                        return new UrlValidationResult(
                            false, Platform, status,
                            $"Found forbidden marker: '{mustNot}'.",
                            EvidenceSnippet: TakeSnippet(body, mustNot)
                        );
                    }
                }

                return new UrlValidationResult(true, Platform, status, null);
            }
            catch (TaskCanceledException)
            {
                return new UrlValidationResult(false, Platform, null, "Timeout while validating URL.");
            }
            catch (HttpRequestException ex)
            {
                return new UrlValidationResult(false, Platform, null, $"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new UrlValidationResult(false, Platform, null, $"Unexpected error: {ex.Message}");
            }
        }

        private static bool ContainsIgnoreCase(string haystack, string needle) =>
            haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);

        private static string? TakeSnippet(string body, string needle)
        {
            var idx = body.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            var start = Math.Max(0, idx - 80);
            var len = Math.Min(body.Length - start, 200);
            return body.Substring(start, len);
        }
    }
}
