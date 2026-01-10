namespace Services.WhatsApp.Abstractions.Login
{
    public interface IWhatsAppMessage
    {
        Task LoginAsync();
        Task SendMessageAsync();
    }
}
