using Configuration.PixVerse;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Infrastructure.PixVerse
{
    public class VerseApiSupport(PixVerseOptions opt, JsonSerializerOptions jsonOpts)
    {
        private readonly PixVerseOptions _opt = opt;
        private readonly JsonSerializerOptions _jsonOpts = jsonOpts;

        internal static string NewRunId() => Guid.NewGuid().ToString("N")[..8];
        public bool TryValidateConfig(out string error)
        {
            if (string.IsNullOrWhiteSpace(_opt.BaseUrl))
            {
                error = "PixVerse BaseUrl is not configured.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            {
                error = "PixVerse ApiKey is not configured.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public Uri BuildEndpoint(string pathOrPathWithId)
        {
            var baseUri = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
            return new Uri(baseUri, pathOrPathWithId.TrimStart('/'));
        }

        public void ApplyAuth(HttpRequestMessage req)
        {
            req.Headers.Remove(ApiConstants.ApiKeyHeader);
            req.Headers.Add(ApiConstants.ApiKeyHeader, _opt.ApiKey);

            if (!req.Headers.Contains(ApiConstants.TraceIdHeader))
                req.Headers.Add(ApiConstants.TraceIdHeader, Guid.NewGuid().ToString());

            req.Headers.Accept.Clear();
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public T? TryDeserialize<T>(string json)
        {
            try { return JsonSerializer.Deserialize<T>(json, _jsonOpts); }
            catch { return default; }
        }
    }
}