namespace Services.WhatsApp.Abstractions.OpenChat
{
    public interface IWhatAppOpenChat
    {
        Task OpenContactChatAsync(
            string chatIdentifier,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default
            );
    }
}
