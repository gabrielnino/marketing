namespace Services.Abstractions.OpenAI.news
{
    public interface INewsHistoryStore
    {
        Task<HashSet<string>> LoadUrlsAsync(CancellationToken ct = default);
        Task AppendUsedUrlsAsync(IEnumerable<string> urls, CancellationToken ct = default);
    }
}
