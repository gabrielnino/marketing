namespace Services.Abstractions.YouTube
{
    public sealed record ChannelDetails(
        string ChannelId,
        string Title,
        DateTimeOffset PublishedAt,
        long SubscriberCount,
        long VideoCount,
        long ViewCount
    );
}
