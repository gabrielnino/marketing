namespace Services.WhatsApp.Abstractions.Search
{
    public interface ISearchBoxTyper
    {
        Task TypeIntoSearchBoxAsync(
            string text,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default);
    }
}
