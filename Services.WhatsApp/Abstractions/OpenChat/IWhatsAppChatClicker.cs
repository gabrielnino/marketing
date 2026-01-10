namespace Services.WhatsApp.Abstractions.XPath
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
