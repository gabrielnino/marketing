namespace Services.Abstractions.YouTube
{
    public sealed record EnrichedVideoData(
        // Basic
        string VideoId,
        string VideoUrl,
        string Title,
        string Description,
        TimeSpan? Duration,
        DateTimeOffset PublishedAt,

        // Statistics
        long Views,
        long Likes,
        long Comments,
        double LikeViewRatio,
        double CommentViewRatio,
        double EngagementScore, // e.g., weighted likes+comments per view

        // Temporal
        TimeSpan Age,
        double ViewsPerHour,     // velocity
        string VelocityBucket,   // e.g., "Exploding", "Rising", "Steady"

        // Metadata
        IReadOnlyList<string> Tags,
        string? CategoryId,
        IReadOnlyList<string> DescriptionKeywords,

        // Channel
        string ChannelId,
        string ChannelTitle,
        long SubscriberCount,
        TimeSpan ChannelAge,
        double ChannelViewsPerVideo // rough baseline
    );
}
