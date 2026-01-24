namespace Services.Abstractions.YouTube
{
    public sealed record SearchResponse(
        string Query,
        IReadOnlyList<SearchVideoItem> Items,
        string? NextPageToken
    );

    public sealed record SearchVideoItem(
        string VideoId,
        string ChannelId,
        string Title,
        string Description,
        DateTimeOffset PublishedAt,
        string? ThumbnailUrl
    );
}
