namespace Services.WhatsApp.Interfaces
{
    public interface IWhatsAppMessage
    {
        Task LoginAsync();
        Task SendMessageAsync();
    }
}
