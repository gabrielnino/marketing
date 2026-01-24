namespace Services.Abstractions.YouTube
{
    public sealed record VideoDetails(
        string VideoId,
        string ChannelId,
        string Title,
        string Description,
        DateTimeOffset PublishedAt,
        TimeSpan? Duration,
        IReadOnlyList<string> Tags,
        string? CategoryId,
        VideoStatistics Statistics
    );

    public sealed record VideoStatistics(
        long ViewCount,
        long LikeCount,
        long CommentCount
    );
}
