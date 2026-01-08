namespace Services.WhatsApp.Interfaces
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
