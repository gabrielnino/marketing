using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Infrastructure.PixVerse
{
    public class PixVerseBase(PixVerseOptions opt)
    {
        public static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        internal static string NewRunId() => Guid.NewGuid().ToString("N")[..8];
        public bool TryValidateConfig(out string error)
        {
            if (string.IsNullOrWhiteSpace(opt.BaseUrl))
            {
                error = "PixVerse BaseUrl is not configured.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(opt.ApiKey))
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
            req.Headers.Remove(Api.ApiKeyHeader);
            req.Headers.Add(Api.ApiKeyHeader, opt.ApiKey);

            if (!req.Headers.Contains(Api.TraceIdHeader))
                req.Headers.Add(Api.TraceIdHeader, Guid.NewGuid().ToString());

            req.Headers.Accept.Clear();
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static T? TryDeserialize<T>(string json)
        {
            try { return JsonSerializer.Deserialize<T>(json, JsonOpts); }
            catch { return default; }
        }
    }
}