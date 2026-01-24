using Application.Result;
using Configuration.YouTube;

namespace Services.Abstractions.YouTube
{


    public interface IYouTubeService
    {
        Task<Operation<SearchResponse>> SearchVideosAsync(string query, SearchOptions options);
        Task<Operation<VideoDetails>> GetVideoDetailsAsync(string videoId);
        Task<Operation<ChannelDetails>> GetChannelDetailsAsync(string channelId);
    }

    public interface IYouTubeViralVideoDiscoverer
    {
        Task<Operation<List<EnrichedVideoData>>> DiscoverAsync(
            string query,
            SearchOptions options,
            CancellationToken ct = default);
    }
}
