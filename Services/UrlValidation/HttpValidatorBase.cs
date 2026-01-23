using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;
using System.Net;
using System.Text;

namespace Services.UrlValidation;

public abstract class HttpValidatorBase(
    HttpClient httpClient,
    IOptionsMonitor<UrlOptions> options) : IUrValidator
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IOptionsMonitor<UrlOptions> _options = options;

    private const string DefaultUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120 Safari/537.36";

    public abstract UrlPlatform Platform { get; }
    protected abstract PlatformRules Rules(UrlOptions opt);

    public async Task<UrlValidationResult> ValidateAsync(string url, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return new UrlValidationResult(false, Platform, null, "Invalid absolute URL.");

        var opt = _options.CurrentValue;
        using var cts = CreateTimeoutCts(opt, ct);

        try
        {
            var headerProbe = await TryFetchHeadersAsync(uri, cts.Token).ConfigureAwait(false);
            if (headerProbe.DecisiveResult is not null)
                return headerProbe.DecisiveResult;

            return await FetchBodySnippetAndEvaluateAsync(uri, opt, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
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

    private async Task<(UrlValidationResult? DecisiveResult, int? StatusCode)> TryFetchHeadersAsync(
        Uri uri,
        CancellationToken ct)
    {
        using var headReq = new HttpRequestMessage(HttpMethod.Head, uri);
        headReq.Headers.UserAgent.ParseAdd(DefaultUserAgent);

        try
        {
            using var headResp = await _httpClient.SendAsync(
                headReq,
                HttpCompletionOption.ResponseHeadersRead,
                ct).ConfigureAwait(false);

            var status = (int)headResp.StatusCode;

            if (headResp.StatusCode == HttpStatusCode.Forbidden)
                return (null, status);

            if ((int)headResp.StatusCode >= 400)
            {
                return (
                    new UrlValidationResult(false, Platform, status, $"HTTP {status} {headResp.ReasonPhrase}"),
                    status);
            }

            return (null, status);
        }
        catch (HttpRequestException)
        {
            return (null, null);
        }
    }

    private async Task<UrlValidationResult> FetchBodySnippetAndEvaluateAsync(
        Uri uri,
        UrlOptions opt,
        CancellationToken ct)
    {
        using var getReq = new HttpRequestMessage(HttpMethod.Get, uri);
        getReq.Headers.UserAgent.ParseAdd(DefaultUserAgent);

        using var resp = await _httpClient.SendAsync(
            getReq,
            HttpCompletionOption.ResponseHeadersRead,
            ct).ConfigureAwait(false);

        var status = (int)resp.StatusCode;

        if ((int)resp.StatusCode >= 400 && resp.StatusCode != HttpStatusCode.Forbidden)
        {
            return new UrlValidationResult(false, Platform, status, $"HTTP {status} {resp.ReasonPhrase}");
        }

        var body = await ReadUpToCharsAsync(resp, opt.MaxBodyCharsToScan, ct).ConfigureAwait(false);
        var rules = Rules(opt);

        foreach (var must in rules.MustContain)
        {
            if (!body.Contains(must, StringComparison.OrdinalIgnoreCase))
            {
                return new UrlValidationResult(false, Platform, status, $"Missing required marker: '{must}'");
            }
        }

        foreach (var bad in rules.MustNotContain)
        {
            if (body.Contains(bad, StringComparison.OrdinalIgnoreCase))
            {
                return new UrlValidationResult(
                    false,
                    Platform,
                    status,
                    $"Contains blocked marker: '{bad}'. Evidence: {TakeSnippet(body, bad)}");
            }
        }

        return new UrlValidationResult(true, Platform, status, "Available");
    }

    private static CancellationTokenSource CreateTimeoutCts(UrlOptions opt, CancellationToken ct)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, opt.TimeoutSeconds)));
        return cts;
    }

    private static async Task<string> ReadUpToCharsAsync(
        HttpResponseMessage resp,
        int maxChars,
        CancellationToken ct)
    {
        await using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var reader = new StreamReader(stream, Encoding.UTF8, true);

        var sb = new StringBuilder(Math.Min(maxChars, 64_000));
        var buffer = new char[8192];

        while (sb.Length < maxChars)
        {
            var read = await reader.ReadAsync(
                buffer.AsMemory(0, Math.Min(buffer.Length, maxChars - sb.Length)), ct)
                .ConfigureAwait(false);

            if (read <= 0) break;
            sb.Append(buffer, 0, read);
        }

        return sb.ToString();
    }

    private static string? TakeSnippet(string body, string needle)
    {
        var idx = body.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;

        var start = Math.Max(0, idx - 80);
        var len = Math.Min(body.Length - start, 200);
        return body.Substring(start, len);
    }
}
