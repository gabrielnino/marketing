using Application.Result;
using Configuration.YouTube;
using Microsoft.Extensions.Logging;
using Services.Abstractions.UrlValidation;
using Services.Abstractions.YouTube;

namespace Services.YouTube
{
    public sealed class YouTubeViralVideoDiscoverer(
        IYouTubeService youTubeService,
        IUrlFactory urlFactory,
        ILogger<YouTubeViralVideoDiscoverer> logger
    ) : IYouTubeViralVideoDiscoverer
    {
        public async Task<Operation<List<EnrichedVideoData>>> DiscoverAsync(
            string query, SearchOptions options, CancellationToken ct = default)
        {
            var searchOp = await youTubeService.SearchVideosAsync(query, options);
            if (!searchOp.IsSuccessful || searchOp.Data is null)
                return searchOp.ConvertTo<List<EnrichedVideoData>>();

            var enriched = new List<EnrichedVideoData>();

            foreach (var item in searchOp.Data.Items)
            {
                ct.ThrowIfCancellationRequested();

                var url = $"https://www.youtube.com/watch?v={item.VideoId}";

                // URL validation: keep YouTubeUrlValidator here ONLY
                var validator = urlFactory.GetValidator(url);
                var validOp = await validator.ValidateAsync(url, ct); // adapt to your validator signature
                if (validOp is not null)
                {
                    if (!validOp.IsValid) continue;
                }
                    

                var videoOp = await youTubeService.GetVideoDetailsAsync(item.VideoId);
                if (!videoOp.IsSuccessful || videoOp.Data is null) continue;

                var channelOp = await youTubeService.GetChannelDetailsAsync(item.ChannelId);
                if (!channelOp.IsSuccessful || channelOp.Data is null) continue;

                enriched.Add(Enrich(videoOp.Data, channelOp.Data, url));
            }

            return Operation<List<EnrichedVideoData>>.Success(enriched, "Discovery completed");
        }

        private static EnrichedVideoData Enrich(VideoDetails v, ChannelDetails c, string url)
        {
            var age = DateTimeOffset.UtcNow - v.PublishedAt;
            var viewsPerHour = age.TotalHours <= 0.01 ? v.Statistics.ViewCount : v.Statistics.ViewCount / age.TotalHours;

            var likeView = v.Statistics.ViewCount == 0 ? 0 : (double)v.Statistics.LikeCount / v.Statistics.ViewCount;
            var commentView = v.Statistics.ViewCount == 0 ? 0 : (double)v.Statistics.CommentCount / v.Statistics.ViewCount;

            var engagementScore = (likeView * 0.7) + (commentView * 0.3);

            var velocityBucket =
                viewsPerHour >= 50_000 ? "Exploding" :
                viewsPerHour >= 10_000 ? "Rising" :
                "Steady";

            var channelAge = DateTimeOffset.UtcNow - c.PublishedAt;
            var channelViewsPerVideo = c.VideoCount == 0 ? 0 : (double)c.ViewCount / c.VideoCount;

            return new EnrichedVideoData(
                v.VideoId,
                url,
                v.Title,
                v.Description,
                v.Duration,
                v.PublishedAt,
                v.Statistics.ViewCount,
                v.Statistics.LikeCount,
                v.Statistics.CommentCount,
                likeView,
                commentView,
                engagementScore,
                age,
                viewsPerHour,
                velocityBucket,
                v.Tags,
                v.CategoryId,
                KeywordExtract(v.Description),
                c.ChannelId,
                c.Title,
                c.SubscriberCount,
                channelAge,
                channelViewsPerVideo
            );
        }

        private static IReadOnlyList<string> KeywordExtract(string text)
        {
            // keep simple and deterministic; LLM can do deeper analysis later
            return [.. text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                       .Select(t => t.Trim().ToLowerInvariant())
                       .Where(t => t.Length >= 5)
                       .Distinct()
                       .Take(25)];
        }
    }
}
