using Domain.WhatsApp;


namespace Services.Interfaces
{
    public interface IWhatsAppChatService
    {
        Task SendMessageAsync(
            ImageMessagePayload imageMessagePayload,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default
            );
    }
}
