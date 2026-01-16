using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Redirect.Func.Functions;

public sealed class RedirectFunction(
    TableServiceClient tableServiceClient,
    QueueServiceClient queueServiceClient,
    IConfiguration cfg,
    ILogger<RedirectFunction> logger)
{
    private readonly TableClient _tableClient = tableServiceClient.GetTableClient("TrackedLinks");
    private readonly QueueClient _queueClient = queueServiceClient.GetQueueClient("trackedlink-visits");
    private readonly IConfiguration _cfg = cfg;
    private readonly ILogger<RedirectFunction> _logger = logger;

    [Function("Redirect")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{code}")]
        HttpRequestData req,
        string code)
    {
        // 1) Validación mínima del código (reduce scanning / abuse)
        if (!RedirectDefense.IsCodeValid(code, _cfg))
        {
            // 404 para no revelar reglas
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        // 2) IP + Rate limiting (defensa mínima DoS)
        var ip = RedirectDefense.GetClientIp(req);
        if (!string.IsNullOrWhiteSpace(ip) && RedirectDefense.IsRateLimited(ip, _cfg))
        {
            return req.CreateResponse(HttpStatusCode.TooManyRequests);
        }

        // 3) Lookup en Table
        TableEntity entity;
        try
        {
            var result = await _tableClient.GetEntityAsync<TableEntity>("LINK", code);
            entity = result.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Table lookup failed for code={Code}", code);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        var targetUrl = entity.GetString("TargetUrl");
        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        // 4) Enqueue visita (no romper redirect si falla)
        _ = EnqueueVisitSafeAsync(code, req);

        // 5) Redirect
        var res = req.CreateResponse(HttpStatusCode.Found);
        res.Headers.Add("Location", targetUrl);
        // Hardening mínimo: no cache del redirect por intermedios
        res.Headers.Add("Cache-Control", "no-store");
        return res;
    }

    private async Task EnqueueVisitSafeAsync(string code, HttpRequestData req)
    {
        try
        {
            await _queueClient.CreateIfNotExistsAsync();

            var ip = RedirectDefense.GetClientIp(req);
            var ipHash = HashIp(ip, _cfg);

            // País si viene de Cloudflare, si no "unknown"
            var country = GetHeader(req, "CF-IPCountry") ?? "unknown";

            // Clasificación mínima bot/human (heurística simple)
            var ua = GetHeader(req, "User-Agent") ?? "";
            var uaClass = IsBotLike(ua) ? "bot_like" : "human_like";

            var payload = new VisitEvent
            {
                Code = code,
                TimestampUtc = DateTime.UtcNow,
                IpHash = ipHash,
                Country = country,
                UaClass = uaClass
            };

            var json = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);

            // Azure Queue acepta texto; usamos Base64 para evitar problemas de caracteres
            await _queueClient.SendMessageAsync(Convert.ToBase64String(bytes));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enqueue visit for code={Code}", code);
        }
    }

    private static string? GetHeader(HttpRequestData req, string name)
    {
        return req.Headers.TryGetValues(name, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private static bool IsBotLike(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return true;

        var ua = userAgent.ToLowerInvariant();
        return ua.Contains("bot")
            || ua.Contains("crawler")
            || ua.Contains("spider")
            || ua.Contains("curl")
            || ua.Contains("wget")
            || ua.Contains("python-requests")
            || ua.Contains("postmanruntime");
    }

    private static string HashIp(string ip, IConfiguration cfg)
    {
        if (string.IsNullOrWhiteSpace(ip) || ip == "unknown") return "unknown";

        var salt = cfg["RedirectDefense:IpHashSalt"];
        if (string.IsNullOrWhiteSpace(salt))
        {
            // sin salt, igual hasheamos pero es menos robusto
            salt = "default_salt_change_me";
        }

        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes($"{salt}|{ip}");
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class VisitEvent
    {
        public required string Code { get; init; }
        public DateTime TimestampUtc { get; init; }
        public string IpHash { get; init; } = "unknown";
        public string Country { get; init; } = "unknown";
        public string UaClass { get; init; } = "unknown";
    }
}
