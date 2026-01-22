namespace Services.Abstractions.Login
{
    public interface IMessage
    {
        Task LoginAsync();
        Task SendMessageAsync();
    }
}
