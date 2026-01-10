namespace Services.WhatsApp.Abstractions.OpenChat
{
    public interface IWhatsAppChatClicker
    {
        Task ClickChatByTitleAsync(
            string chatTitle,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default);
    }
}
