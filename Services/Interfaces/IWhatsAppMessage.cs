namespace Services.Interfaces
{
    public interface IWhatsAppMessage
    {
        Task LoginAsync();
        Task SendMessageAsync();
    }
}
