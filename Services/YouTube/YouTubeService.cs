using Application.Result;
using Configuration.YouTube;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions.YouTube;
using System.Net;
using System.Text.Json;

namespace Services.YouTube
{
    public sealed class YouTubeService(
        HttpClient httpClient,
        IOptions<YouTubeApiOptions> options,
        IErrorHandler errorHandler,
        ILogger<YouTubeService> logger
    ) : IYouTubeService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly YouTubeApiOptions _cfg = options?.Value ?? throw new ArgumentNullException(nameof(options));
        private readonly IErrorHandler _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        private readonly ILogger<YouTubeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<Operation<SearchResponse>> SearchVideosAsync(string query, SearchOptions options)
        {
            var requestId = Guid.NewGuid().ToString("N");
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Service"] = nameof(YouTubeService),
                ["Method"] = nameof(SearchVideosAsync),
                ["RequestId"] = requestId
            });

            // STEP 0: Enter
            _logger.LogInformation("STEP 0: Enter SearchVideosAsync. QueryLen={Len}", query?.Length ?? 0);

            try
            {
                // STEP 1: Validate inputs
                _logger.LogDebug("STEP 1: Validating inputs...");
                if (string.IsNullOrWhiteSpace(query))
                {
                    _logger.LogWarning("STEP 1: Validation failed. Reason=EmptyQuery");
                    return _errorHandler.Business<SearchResponse>("Query cannot be null or empty.");
                }

                if (options is null)
                {
                    _logger.LogWarning("STEP 1: Validation failed. Reason=NullSearchOptions");
                    return _errorHandler.Business<SearchResponse>("SearchOptions cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(_cfg.ApiKey))
                {
                    _logger.LogError("STEP 1: Validation failed. Reason=MissingApiKey");
                    return _errorHandler.Fail<SearchResponse>(
                        new InvalidOperationException("YouTube API key is missing."),
                        "YouTube API key is not configured.");
                }

                _logger.LogDebug("STEP 1: Validation passed.");

                // STEP 2: Normalize defaults
                _logger.LogDebug("STEP 2: Normalizing defaults...");
                var maxResults = options.MaxResults <= 0 ? 10 : options.MaxResults;
                var order = string.IsNullOrWhiteSpace(options.Order) ? "viewCount" : options.Order;
                var safeSearch = string.IsNullOrWhiteSpace(options.SafeSearch) ? "none" : options.SafeSearch;

                _logger.LogInformation(
                    "STEP 2: Defaults applied. MaxResults={MaxResults}, Order={Order}, SafeSearch={SafeSearch}, Region={Region}, Lang={Lang}, After={After}, Before={Before}",
                    maxResults,
                    order,
                    safeSearch,
                    options.RegionCode,
                    options.RelevanceLanguage,
                    options.PublishedAfterIso,
                    options.PublishedBeforeIso);

                // STEP 3: Build URI (do not log raw URI because it contains API key)
                _logger.LogDebug("STEP 3: Building request URI (redacted)...");
                var uri = BuildSearchUri(query, options, maxResults, order, safeSearch);
                _logger.LogInformation("STEP 3: URI built (redacted). Endpoint=search?part=snippet&type=video&...");

                // STEP 4: Call HTTP
                _logger.LogInformation("STEP 4: Calling YouTube API via HttpClient...");
                using var resp = await _httpClient.GetAsync(uri);
                _logger.LogInformation("STEP 4: HTTP call completed. StatusCode={StatusCode} ({StatusInt})",
                    resp.StatusCode, (int)resp.StatusCode);

                // STEP 5: Read body once
                _logger.LogDebug("STEP 5: Reading response body...");
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogInformation("STEP 5: Body read. BodyLen={BodyLen}", body?.Length ?? 0);

                // STEP 6: Handle auth/quota failures
                if (resp.StatusCode == HttpStatusCode.Forbidden || resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning(
                        "STEP 6: Auth/quota failure detected. StatusCode={StatusCode} ({StatusInt})",
                        resp.StatusCode, (int)resp.StatusCode);

                    return _errorHandler.Fail<SearchResponse>(
                        new InvalidOperationException(
                            $"YouTube API auth/quota failure. Status={(int)resp.StatusCode}. Body={body}"),
                        "YouTube API rejected the request (auth/quota).");
                }

                // STEP 7: Handle non-success
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "STEP 7: Non-success status detected. StatusCode={StatusCode} ({StatusInt})",
                        resp.StatusCode, (int)resp.StatusCode);

                    return _errorHandler.Fail<SearchResponse>(
                        new HttpRequestException(
                            $"YouTube API error. Status={(int)resp.StatusCode}. Body={body}"),
                        "YouTube API returned non-success status.");
                }

                // STEP 8: Deserialize JSON
                _logger.LogDebug("STEP 8: Deserializing JSON response...");
                YouTubeSearchListDto? dto;
                try
                {
                    dto = JsonSerializer.Deserialize<YouTubeSearchListDto>(body, JsonOptions);
                }
                catch (JsonException jex)
                {
                    _logger.LogError(jex, "STEP 8: JSON deserialization failed.");
                    return _errorHandler.Fail<SearchResponse>(jex, "Failed to parse YouTube response.");
                }

                var rawItems = dto?.Items?.Count ?? 0;
                _logger.LogInformation("STEP 8: JSON deserialized. RawItems={RawItems}, NextPageTokenPresent={HasToken}",
                    rawItems,
                    !string.IsNullOrWhiteSpace(dto?.NextPageToken));

                // STEP 9: Map to domain model
                _logger.LogDebug("STEP 9: Mapping DTO -> SearchResponse...");
                var items =
                    (dto?.Items ?? [])
                    .Where(x => x?.Id?.VideoId is not null)
                    .Select(x => new SearchVideoItem(
                        VideoId: x!.Id!.VideoId!,
                        ChannelId: x.Snippet?.ChannelId ?? string.Empty,
                        Title: x.Snippet?.Title ?? string.Empty,
                        Description: x.Snippet?.Description ?? string.Empty,
                        PublishedAt: x.Snippet?.PublishedAt ?? DateTimeOffset.MinValue,
                        ThumbnailUrl: x.Snippet?.Thumbnails?.High?.Url
                                      ?? x.Snippet?.Thumbnails?.Medium?.Url
                                      ?? x.Snippet?.Thumbnails?.Default?.Url
                    ))
                    .ToList();

                var mapped = new SearchResponse(
                    Query: query,
                    Items: items,
                    NextPageToken: dto?.NextPageToken
                );

                _logger.LogInformation(
                    "STEP 9: Mapping completed. MappedItems={Count}",
                    mapped.Items.Count);

                // STEP 10: Return success
                _logger.LogInformation("STEP 10: Returning success.");
                return Operation<SearchResponse>.Success(mapped, "Search completed");
            }
            catch (Exception ex)
            {
                // STEP X: Fail
                _logger.LogError(ex, "STEP X: Unhandled exception in SearchVideosAsync.");
                return _errorHandler.Fail<SearchResponse>(ex, "SearchVideosAsync failed.");
            }
        }

        public Task<Operation<VideoDetails>> GetVideoDetailsAsync(string videoId)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Service"] = nameof(YouTubeService),
                ["Method"] = nameof(GetVideoDetailsAsync)
            });

            _logger.LogInformation("STEP 0: Enter GetVideoDetailsAsync. VideoIdEmpty={Empty}", string.IsNullOrWhiteSpace(videoId));

            try
            {
                _logger.LogDebug("STEP 1: Validating inputs...");
                if (string.IsNullOrWhiteSpace(videoId))
                {
                    _logger.LogWarning("STEP 1: Validation failed. Reason=EmptyVideoId");
                    return Task.FromResult(_errorHandler.Business<VideoDetails>("VideoId cannot be null or empty."));
                }

                if (string.IsNullOrWhiteSpace(_cfg.ApiKey))
                {
                    _logger.LogError("STEP 1: Validation failed. Reason=MissingApiKey");
                    return Task.FromResult(_errorHandler.Fail<VideoDetails>(
                        new InvalidOperationException("YouTube API key is missing."),
                        "YouTube API key is not configured."));
                }

                _logger.LogWarning("STEP 2: Not implemented.");
                return Task.FromResult(_errorHandler.Business<VideoDetails>("GetVideoDetailsAsync is not implemented yet."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "STEP X: Unhandled exception in GetVideoDetailsAsync.");
                return Task.FromResult(_errorHandler.Fail<VideoDetails>(ex, "GetVideoDetailsAsync failed."));
            }
        }

        public Task<Operation<ChannelDetails>> GetChannelDetailsAsync(string channelId)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Service"] = nameof(YouTubeService),
                ["Method"] = nameof(GetChannelDetailsAsync)
            });

            _logger.LogInformation("STEP 0: Enter GetChannelDetailsAsync. ChannelIdEmpty={Empty}", string.IsNullOrWhiteSpace(channelId));

            try
            {
                _logger.LogDebug("STEP 1: Validating inputs...");
                if (string.IsNullOrWhiteSpace(channelId))
                {
                    _logger.LogWarning("STEP 1: Validation failed. Reason=EmptyChannelId");
                    return Task.FromResult(_errorHandler.Business<ChannelDetails>("ChannelId cannot be null or empty."));
                }

                if (string.IsNullOrWhiteSpace(_cfg.ApiKey))
                {
                    _logger.LogError("STEP 1: Validation failed. Reason=MissingApiKey");
                    return Task.FromResult(_errorHandler.Fail<ChannelDetails>(
                        new InvalidOperationException("YouTube API key is missing."),
                        "YouTube API key is not configured."));
                }

                _logger.LogWarning("STEP 2: Not implemented.");
                return Task.FromResult(_errorHandler.Business<ChannelDetails>("GetChannelDetailsAsync is not implemented yet."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "STEP X: Unhandled exception in GetChannelDetailsAsync.");
                return Task.FromResult(_errorHandler.Fail<ChannelDetails>(ex, "GetChannelDetailsAsync failed."));
            }
        }

        private string BuildSearchUri(string query, SearchOptions options, int maxResults, string order, string safeSearch)
        {
            // NOTE: _httpClient.BaseAddress should already be "https://www.googleapis.com/youtube/v3/"
            // Avoid logging this URI externally (it contains the API key).
            return
                $"search?part=snippet" +
                $"&type=video" +
                $"&q={Uri.EscapeDataString(query)}" +
                $"&maxResults={maxResults}" +
                $"&order={Uri.EscapeDataString(order)}" +
                $"&safeSearch={Uri.EscapeDataString(safeSearch)}" +
                $"{(string.IsNullOrWhiteSpace(options.RegionCode) ? "" : $"&regionCode={Uri.EscapeDataString(options.RegionCode)}")}" +
                $"{(string.IsNullOrWhiteSpace(options.RelevanceLanguage) ? "" : $"&relevanceLanguage={Uri.EscapeDataString(options.RelevanceLanguage)}")}" +
                $"{(string.IsNullOrWhiteSpace(options.PublishedAfterIso) ? "" : $"&publishedAfter={Uri.EscapeDataString(options.PublishedAfterIso)}")}" +
                $"{(string.IsNullOrWhiteSpace(options.PublishedBeforeIso) ? "" : $"&publishedBefore={Uri.EscapeDataString(options.PublishedBeforeIso)}")}" +
                $"&key={Uri.EscapeDataString(_cfg.ApiKey)}";
        }
    }

    // DTOs unchanged
    public sealed class YouTubeSearchListDto
    {
        public string? NextPageToken { get; set; }
        public List<YouTubeSearchItemDto>? Items { get; set; }
    }

    public sealed class YouTubeSearchItemDto
    {
        public YouTubeSearchIdDto? Id { get; set; }
        public YouTubeSearchSnippetDto? Snippet { get; set; }
    }

    public sealed class YouTubeSearchIdDto
    {
        public string? VideoId { get; set; }
    }

    public sealed class YouTubeSearchSnippetDto
    {
        public DateTimeOffset? PublishedAt { get; set; }
        public string? ChannelId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public YouTubeThumbnailsDto? Thumbnails { get; set; }
    }

    public sealed class YouTubeThumbnailsDto
    {
        public YouTubeThumbnailDto? Default { get; set; }
        public YouTubeThumbnailDto? Medium { get; set; }
        public YouTubeThumbnailDto? High { get; set; }
    }

    public sealed class YouTubeThumbnailDto
    {
        public string? Url { get; set; }
    }
}
