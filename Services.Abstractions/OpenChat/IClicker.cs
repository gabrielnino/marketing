namespace Services.Abstractions.OpenChat
{
    public interface IClicker
    {
        Task ClickChatByTitleAsync(
            string chatTitle,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default);
    }
}
