namespace Services.WhatsApp.Abstractions.Login
{
    public interface IMessage
    {
        Task LoginAsync();
        Task SendMessageAsync();
    }
}
