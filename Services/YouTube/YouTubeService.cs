using Application.Result;
using Configuration.YouTube;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions.YouTube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.YouTube
{
    public sealed class YouTubeService(
        HttpClient httpClient,
        IOptions<YouTubeApiOptions> options,
        IErrorHandler errorHandler,
        ILogger<YouTubeService> logger
    ) : IYouTubeService
    {
        private HttpClient _httpClient = httpClient;
        private IOptions<YouTubeApiOptions> _options = options;
        private IErrorHandler _errorHandler = errorHandler;
        private ILogger<YouTubeService> _logger= logger;

        public async Task<Operation<SearchResponse>> SearchVideosAsync(string query, SearchOptions options)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return _errorHandler.Business<SearchResponse>("Query cannot be null or empty.");

                // Defaults (defensive)
                var maxResults = options.MaxResults <= 0 ? 10 : options.MaxResults;
                var order = string.IsNullOrWhiteSpace(options.Order) ? "viewCount" : options.Order;
                var safeSearch = string.IsNullOrWhiteSpace(options.SafeSearch) ? "none" : options.SafeSearch;

                // Build request to: https://www.googleapis.com/youtube/v3/search
                // NOTE: _httpClient.BaseAddress should already be "https://www.googleapis.com/youtube/v3/"
                var uri =
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
                    //$"{(string.IsNullOrWhiteSpace(options.VideoCategoryId) ? "" : $"&videoCategoryId={Uri.EscapeDataString(options.VideoCategoryId)}")}" +
                    //$"{(string.IsNullOrWhiteSpace(options.ChannelId) ? "" : $"&channelId={Uri.EscapeDataString(options.ChannelId)}")}" +
                    $"&key={Uri.EscapeDataString(_options.Value.ApiKey)}";

                using var resp = await _httpClient.GetAsync(uri);

                if (resp.StatusCode == HttpStatusCode.Forbidden || resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    return _errorHandler.Fail<SearchResponse>(
                        new InvalidOperationException($"YouTube API auth/quota failure. Status={(int)resp.StatusCode}. Body={body}"),
                        "YouTube API rejected the request (auth/quota).");
                }

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    return _errorHandler.Fail<SearchResponse>(
                        new HttpRequestException($"YouTube API error. Status={(int)resp.StatusCode}. Body={body}"),
                        "YouTube API returned non-success status.");
                }

                var json = await resp.Content.ReadAsStringAsync();

                var dto = JsonSerializer.Deserialize<YouTubeSearchListDto>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var items =
                    (dto?.Items ?? new List<YouTubeSearchItemDto>())
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

                return Operation<SearchResponse>.Success(mapped, "Search completed");
            }
            catch (Exception ex)
            {
                return _errorHandler.Fail<SearchResponse>(ex, "SearchVideosAsync failed.");
            }
        }
        public async Task<Operation<VideoDetails>> GetVideoDetailsAsync(string videoId)
        {
            try
            {
                // https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics,contentDetails&id=...
                return Operation<VideoDetails>.Success(/*mapped*/ null!, "Video details fetched");
            }
            catch (Exception ex)
            {
                errorHandler.Fail<Operation<VideoDetails>>(ex);
                var strategy = new BusinessStrategy<VideoDetails>();
                return OperationStrategy<VideoDetails>.Fail(ex.Message, strategy);
            }
        }

        public async Task<Operation<ChannelDetails>> GetChannelDetailsAsync(string channelId)
        {
            try
            {
                // https://www.googleapis.com/youtube/v3/channels?part=snippet,statistics&id=...
                return Operation<ChannelDetails>.Success(/*mapped*/ null!, "Channel details fetched");
            }
            catch (Exception ex)
            {
                errorHandler.Fail<Operation<ChannelDetails>>(ex);
                var strategy = new BusinessStrategy<ChannelDetails>();
                return OperationStrategy<ChannelDetails>.Fail(ex.Message, strategy);
            }
        }
    }

    // -----------------------------
    // Minimal DTOs for /search (YouTube Data API v3)
    // -----------------------------
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
