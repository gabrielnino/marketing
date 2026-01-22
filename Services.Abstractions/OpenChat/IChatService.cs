using Domain.WhatsApp;


namespace Services.Abstractions.OpenChat
{
    public interface IChatService
    {
        Task SendMessageAsync(
            ImageMessagePayload imageMessagePayload,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default
            );
    }
}
