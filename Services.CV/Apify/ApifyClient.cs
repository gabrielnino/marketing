using Application.Apify.Models;
using Application.Result;
using Configuration.Apify;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.CV.Abstractions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services.CV.Apify
{
    public class ApifyClient(
        HttpClient httpClient,
        IOptions<ApifyOptions> options,
        ILogger<ApifyClient> logger,
        IErrorHandler errorHandler
    ) : IApifyClient
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ApifyOptions _options = options.Value;
        private readonly ILogger<ApifyClient> _logger = logger;
        private readonly IErrorHandler _error = errorHandler;

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // --------------------------------------------------------------------------------
        // Helper
        // --------------------------------------------------------------------------------
        private string BuildUrl(string path, string? query = null)
        {
            var sb = new StringBuilder($"{_options.BaseUrl}/{path}");
            sb.Append(path.Contains('?') ? "&" : "?");
            sb.Append($"token={_options.ApiKey}");
            if (!string.IsNullOrWhiteSpace(query))
            {
                sb.Append('&');
                sb.Append(query);
            }
            return sb.ToString();
        }

        private async Task<Operation<T>> GetAsync<T>(string url, CancellationToken ct, bool unwrap = true)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Apify GET failed. Url={Url}, Status={Status}, Content={Content}", url.Replace(_options.ApiKey, "***"), response.StatusCode, err);
                    return _error.Fail<T>(null, $"Apify request failed: {response.StatusCode}");
                }
                
                if (typeof(T) == typeof(string))
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    return Operation<T>.Success((T)(object)content);
                }

                // For generic objects
                var json = await response.Content.ReadAsStringAsync(ct);

                if (unwrap)
                {
                    var wrapperObj = JsonSerializer.Deserialize(json, typeof(ApifyResponse<T>), _jsonOptions);
                    if (wrapperObj is ApifyResponse<T> wrapper && wrapper.Data != null)
                    {
                        return Operation<T>.Success(wrapper.Data);
                    }
                    return _error.Fail<T>(null, "Apify response data was empty.");
                }
                else
                {
                    var dataObj = JsonSerializer.Deserialize(json, typeof(T), _jsonOptions);
                    if (dataObj is T data)
                    {
                        return Operation<T>.Success(data);
                    }
                    return _error.Fail<T>(null, "Apify response was empty.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Apify GET exception.");
                return _error.Fail<T>(ex, "Apify request error.");
            }
        }

        private async Task<Operation<T>> PostAsync<T>(string url, object? payload, CancellationToken ct, bool unwrap = true)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, payload ?? new { }, _jsonOptions, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Apify POST failed. Url={Url}, Status={Status}, Content={Content}", url.Replace(_options.ApiKey, "***"), response.StatusCode, err);
                    return _error.Fail<T>(null, $"Apify request failed: {response.StatusCode}");
                }

                if (typeof(T) == typeof(bool)) return Operation<T>.Success((T)(object)true);

                var json = await response.Content.ReadAsStringAsync(ct);

                if (unwrap)
                {
                    var wrapperObj = JsonSerializer.Deserialize(json, typeof(ApifyResponse<T>), _jsonOptions);
                    if (wrapperObj is ApifyResponse<T> wrapper && wrapper.Data != null)
                    {
                        return Operation<T>.Success(wrapper.Data);
                    }
                    return _error.Fail<T>(null, "Apify response data was empty.");
                }
                else
                {
                    var dataObj = JsonSerializer.Deserialize(json, typeof(T), _jsonOptions);
                     if (dataObj is T data)
                    {
                        return Operation<T>.Success(data);
                    }
                    return _error.Fail<T>(null, "Apify response was empty.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Apify POST exception.");
                return _error.Fail<T>(ex, "Apify request error.");
            }
        }
        
        private async Task<Operation<bool>> DeleteAsync(string url, CancellationToken ct)
        {
             try
            {
                var response = await _httpClient.DeleteAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Apify DELETE failed. Url={Url}, Status={Status}, Content={Content}", url.Replace(_options.ApiKey, "***"), response.StatusCode, err);
                    return _error.Fail<bool>(null, $"Apify delete failed: {response.StatusCode}");
                }
                return Operation<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Apify DELETE exception.");
                return _error.Fail<bool>(ex, "Apify request error.");
            }
        }

        // --------------------------------------------------------------------------------
        // Builds
        // --------------------------------------------------------------------------------

        public Task<Operation<PaginatedList<ActorBuild>>> GetBuildsAsync(int limit = 20, int offset = 0, bool desc = false, CancellationToken ct = default)
        {
            var query = $"limit={limit}&offset={offset}&desc={(desc ? 1 : 0)}";
            return GetAsync<PaginatedList<ActorBuild>>(BuildUrl("actor-builds", query), ct);
        }

        public Task<Operation<ActorBuild>> GetBuildAsync(string buildId, bool waitForFinish = false, CancellationToken ct = default)
        {
            var query = waitForFinish ? "waitForFinish=1" : null;
            return GetAsync<ActorBuild>(BuildUrl($"actor-builds/{buildId}", query), ct);
        }

        public Task<Operation<bool>> DeleteBuildAsync(string buildId, CancellationToken ct = default)
        {
            return DeleteAsync(BuildUrl($"actor-builds/{buildId}"), ct);
        }

        public Task<Operation<ActorBuild>> AbortBuildAsync(string buildId, CancellationToken ct = default)
        {
            // Abort is POST
            return PostAsync<ActorBuild>(BuildUrl($"actor-builds/{buildId}/abort"), null, ct);
        }

        public Task<Operation<string>> GetBuildLogAsync(string buildId, CancellationToken ct = default)
        {
            return GetAsync<string>(BuildUrl($"actor-builds/{buildId}/log"), ct);
        }

        public Task<Operation<string>> GetBuildOpenApiDefAsync(string buildId, CancellationToken ct = default)
        {
            return GetAsync<string>(BuildUrl($"actor-builds/{buildId}/openapi.json"), ct);
        }

        // --------------------------------------------------------------------------------
        // Runs
        // --------------------------------------------------------------------------------

        public Task<Operation<PaginatedList<ActorRun>>> GetRunsAsync(int limit = 20, int offset = 0, bool desc = false, CancellationToken ct = default)
        {
            var query = $"limit={limit}&offset={offset}&desc={(desc ? 1 : 0)}";
            return GetAsync<PaginatedList<ActorRun>>(BuildUrl("actor-runs", query), ct);
        }

        public Task<Operation<ActorRun>> GetRunAsync(string runId, bool waitForFinish = false, CancellationToken ct = default)
        {
             var query = waitForFinish ? "waitForFinish=1" : null;
             return GetAsync<ActorRun>(BuildUrl($"actor-runs/{runId}", query), ct);
        }

        public async Task<Operation<ActorRun>> UpdateRunAsync(string runId, ActorRunUpdate update, CancellationToken ct = default)
        {
             try
            {
                var url = BuildUrl($"actor-runs/{runId}");
                var response = await _httpClient.PutAsJsonAsync(url, update, _jsonOptions, ct);
                 if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Apify PUT failed. Url={Url}, Status={Status}, Content={Content}", url.Replace(_options.ApiKey, "***"), response.StatusCode, err);
                    return _error.Fail<ActorRun>(null, $"Apify update failed: {response.StatusCode}");
                }
                var json = await response.Content.ReadAsStringAsync(ct);
                var wrapperObj = JsonSerializer.Deserialize(json, typeof(ApifyResponse<ActorRun>), _jsonOptions);
                if (wrapperObj is ApifyResponse<ActorRun> wrapper && wrapper.Data != null)
                {
                    return Operation<ActorRun>.Success(wrapper.Data);
                }
                return _error.Fail<ActorRun>(null, "Apify response data was empty.");
            }
            catch (Exception ex)
            {
                return _error.Fail<ActorRun>(ex, "Apify update error.");
            }
        }

        public Task<Operation<bool>> DeleteRunAsync(string runId, CancellationToken ct = default)
        {
             return DeleteAsync(BuildUrl($"actor-runs/{runId}"), ct);
        }

        public Task<Operation<ActorRun>> AbortRunAsync(string runId, CancellationToken ct = default)
        {
             return PostAsync<ActorRun>(BuildUrl($"actor-runs/{runId}/abort"), null, ct);
        }

        public Task<Operation<ActorRun>> MetamorphRunAsync(string runId, string targetActorId, object? input = null, string? build = null, CancellationToken ct = default)
        {
            var query = $"targetActorId={targetActorId}";
            if(!string.IsNullOrEmpty(build)) query += $"&build={build}";
            
            return PostAsync<ActorRun>(BuildUrl($"actor-runs/{runId}/metamorph", query), input, ct);
        }

        public Task<Operation<ActorRun>> RebootRunAsync(string runId, CancellationToken ct = default)
        {
             return PostAsync<ActorRun>(BuildUrl($"actor-runs/{runId}/reboot"), null, ct);
        }

        public Task<Operation<ActorRun>> ResurrectRunAsync(string runId, CancellationToken ct = default)
        {
             return PostAsync<ActorRun>(BuildUrl($"actor-runs/{runId}/resurrect"), null, ct);
        }

        public async Task<Operation<bool>> ChargeRunAsync(string runId, string eventName, int count = 1, CancellationToken ct = default)
        {
            // POST to /charge, generic structure? Doc says "charge for events". payload usually { "eventName": "foo", "count": 1 }
            // Apify docs say: request body: { "eventName": "my-event", "count": 10 }
            var payload = new { eventName, count };
             try
            {
                var url = BuildUrl($"actor-runs/{runId}/charge");
                var response = await _httpClient.PostAsJsonAsync(url, payload, _jsonOptions, ct);
                if (!response.IsSuccessStatusCode)
                {
                     var err = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Apify Charge failed. Url={Url}, Status={Status}, Content={Content}", url.Replace(_options.ApiKey, "***"), response.StatusCode, err);
                    return _error.Fail<bool>(null, $"Apify charge failed: {response.StatusCode}");
                }
                return Operation<bool>.Success(true);
            }
             catch (Exception ex)
            {
                return _error.Fail<bool>(ex, "Apify charge error.");
            }
        }

        public Task<Operation<List<Dictionary<string, object>>>> GetRunDatasetItemsAsync(string runId, CancellationToken ct = default)
        {
            // /v2/actor-runs/{runId}/dataset/items -> Returns array, NO wrapper
            return GetAsync<List<Dictionary<string, object>>>(BuildUrl($"actor-runs/{runId}/dataset/items"), ct, unwrap: false);
        }

        // --------------------------------------------------------------------------------
        // Datasets
        // --------------------------------------------------------------------------------

        public Task<Operation<PaginatedList<Dataset>>> GetDatasetsAsync(int limit = 20, int offset = 0, bool desc = false, bool unnamed = false, CancellationToken ct = default)
        {
            var query = $"limit={limit}&offset={offset}&desc={(desc ? 1 : 0)}&unnamed={(unnamed ? 1 : 0)}";
            return GetAsync<PaginatedList<Dataset>>(BuildUrl("datasets", query), ct);
        }

        public Task<Operation<List<DatasetItem>>> GetDatasetItemsAsync(string datasetId, int limit = 1000, int offset = 0, CancellationToken ct = default)
        {
             var query = $"limit={limit}&offset={offset}";
             return GetAsync<List<DatasetItem>>(BuildUrl($"datasets/{datasetId}/items", query), ct, unwrap: false);
        }
    }
}
