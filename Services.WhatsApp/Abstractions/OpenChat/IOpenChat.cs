namespace Services.WhatsApp.Abstractions.OpenChat
{
    public interface IOpenChat
    {
        Task OpenContactChatAsync(
            string chatIdentifier,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default
            );
    }
}
