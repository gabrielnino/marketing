using Application.PixVerse.Response;
using Application.Result;
using Configuration.PixVerse;
using Infrastructure.PixVerse.Constants;
using Infrastructure.PixVerse.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Infrastructure.PixVerse.Http
{
    public class PixVerseRequestHandler(
        HttpClient httpClient,
        IOptions<PixVerseOptions> options,
        ILogger<PixVerseRequestHandler> logger,
        IErrorHandler errorHandler) : IPixVerseRequestHandler
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly PixVerseOptions _options = options.Value;
        private readonly ILogger<PixVerseRequestHandler> _logger = logger;
        private readonly IErrorHandler _errorHandler = errorHandler;

        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public async Task<Operation<T>> GetAsync<T>(string path, CancellationToken ct = default)
        {
            var runId = NewRunId();
            _logger.LogInformation("[RUN {RunId}] START GET request. Path={Path}", runId, path);
            
            if (!ValidateConfig(out var configError))
                return _errorHandler.Fail<T>(null, configError);

            var endpoint = BuildEndpoint(path);
            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            ApplyAuth(req);

            return await ExecuteRequestAsync<T>(req, runId, ct);
        }

        public async Task<Operation<T>> PostAsync<T>(string path, HttpContent? content, CancellationToken ct = default)
        {
            var runId = NewRunId();
            _logger.LogInformation("[RUN {RunId}] START POST request. Path={Path}", runId, path);

            if (!ValidateConfig(out var configError))
                return _errorHandler.Fail<T>(null, configError);

            var endpoint = BuildEndpoint(path);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Content = content;
            ApplyAuth(req);

            return await ExecuteRequestAsync<T>(req, runId, ct);
        }

        public async Task<Operation<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken ct = default)
        {
            // If the caller constructs the request, they are responsible for setting the URI correctly or we assume it's already correct.
            // But we still need to apply Auth if it's missing, or assume the caller did it. 
            // Ideally, we want to control the Auth application.
            // For flexibility, let's assume this method is for non-standard flows, but we'll try to apply auth if it looks like an API call.
            
            var runId = NewRunId();
            _logger.LogInformation("[RUN {RunId}] START SendAsync request. Uri={Uri}", runId, request.RequestUri);

            ApplyAuth(request);
            
            return await ExecuteRequestAsync<T>(request, runId, ct);
        }

        private async Task<Operation<T>> ExecuteRequestAsync<T>(HttpRequestMessage req, string runId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("[RUN {RunId}] Sending request...", runId);
                using var res = await _httpClient.SendAsync(req, ct);

                _logger.LogInformation("[RUN {RunId}] Response received. StatusCode={StatusCode}", runId, (int)res.StatusCode);

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[RUN {RunId}] FAILED Non-success status.", runId);
                    return _errorHandler.Fail<T>(null, $"Request failed. HTTP {(int)res.StatusCode}");
                }

                var json = await res.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("[RUN {RunId}] BodyLength={Length}", runId, json.Length);

                // Try envelope first
                var env = TryDeserialize<Envelope<T>>(json);
                if (env is not null && (env.ErrCode != 0 || env.Resp != null))
                {
                   if (env.ErrCode != 0)
                    {
                        _logger.LogWarning("[RUN {RunId}] PixVerse error. Code={Code} Msg={Msg}", runId, env.ErrCode, env.ErrMsg);
                        return _errorHandler.Fail<T>(null, $"PixVerse error {env.ErrCode}: {env.ErrMsg}");
                    }
                    
                    if (env.Resp == null)
                    {
                         // Sometimes resp is null but no error code? Handle as failure or try raw.
                         // Usually if ErrCode == 0, Resp should be there.
                         _logger.LogWarning("[RUN {RunId}] Envelope parsed but Resp is null.", runId);
                    }
                    else
                    {
                        return Operation<T>.Success(env.Resp, env.ErrMsg);
                    }
                }

                // Fallback to raw deserialization
                _logger.LogInformation("[RUN {RunId}] Fallback to raw model deserialization.", runId);
                var raw = TryDeserialize<T>(json);
                if (raw is null)
                {
                     _logger.LogWarning("[RUN {RunId}] Failed to deserialize response.", runId);
                     return _errorHandler.Fail<T>(null, "Invalid response payload.");
                }

                return Operation<T>.Success(raw);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RUN {RunId}] Request execution failed.", runId);
                return _errorHandler.Fail<T>(ex, "Request execution failed");
            }
        }
        
        public async Task<Operation<FileInfo>> DownloadFileAsync(string url, string destinationPath, CancellationToken ct = default)
        {
            var runId = NewRunId();
            _logger.LogInformation("[RUN {RunId}] START DownloadFileAsync Url={Url} Dest={Dest}", runId, url, destinationPath);
            
            try 
            {
                 // Handle directory vs file path logic
                string finalPath = ResolveFinalPath(destinationPath, url);
                var dir = Path.GetDirectoryName(finalPath);
                if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);

                var tmpPath = finalPath + ".download.tmp";

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                // ApplyAuth(req); // Check if auth is needed for CDN? Usually not for public S3/CDN links, but let's be careful.
                // In the original code, it was commented out for download.

                using var res = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
                if (!res.IsSuccessStatusCode)
                {
                     return _errorHandler.Fail<FileInfo>(null, $"Download failed. HTTP {(int)res.StatusCode}");
                }

                await using var httpStream = await res.Content.ReadAsStreamAsync(ct);
                await using var fileStream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
                
                await httpStream.CopyToAsync(fileStream, ct);
                await fileStream.FlushAsync(ct);
                
                // Move logic
                if (File.Exists(finalPath)) File.Delete(finalPath);
                File.Move(tmpPath, finalPath);
                
                return Operation<FileInfo>.Success(new FileInfo(finalPath));
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "[RUN {RunId}] Download failed.", runId);
                 // Cleanup
                 try { if (File.Exists(destinationPath + ".download.tmp")) File.Delete(destinationPath + ".download.tmp"); } catch { }
                 return _errorHandler.Fail<FileInfo>(ex, "Download failed");
            }
        }

        private string ResolveFinalPath(string destinationFilePath, string url)
        {
             if (Directory.Exists(destinationFilePath) ||
                 destinationFilePath.EndsWith(Path.DirectorySeparatorChar) ||
                 destinationFilePath.EndsWith(Path.AltDirectorySeparatorChar))
            {
                // Try to infer filename from URL or generate one
                var fileName = Path.GetFileName(new Uri(url).LocalPath);
                if (string.IsNullOrWhiteSpace(fileName)) fileName = $"download_{Guid.NewGuid()}.mp4"; // Fallback
                return Path.Combine(destinationFilePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), fileName);
            }
            return destinationFilePath;
        }

        private bool ValidateConfig(out string error)
        {
            if (string.IsNullOrWhiteSpace(_options.BaseUrl)) { error = "BaseUrl missig"; return false; }
            if (string.IsNullOrWhiteSpace(_options.ApiKey)) { error = "ApiKey missing"; return false; }
            error = string.Empty;
            return true;
        }

        private Uri BuildEndpoint(string path)
        {
            var baseUri = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
            return new Uri(baseUri, path.TrimStart('/'));
        }

        private void ApplyAuth(HttpRequestMessage req)
        {
            if (!req.Headers.Contains(Api.ApiKeyHeader))
                req.Headers.Add(Api.ApiKeyHeader, _options.ApiKey);
            
            if (!req.Headers.Contains(Api.TraceIdHeader))
                req.Headers.Add(Api.TraceIdHeader, Guid.NewGuid().ToString());

             req.Headers.Accept.Clear();
             req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static string NewRunId() => Guid.NewGuid().ToString("N")[..8];

        private static T? TryDeserialize<T>(string json)
        {
            try { return JsonSerializer.Deserialize<T>(json, _jsonOptions); }
            catch { return default; }
        }
    }
}
