namespace Services.WhatsApp.Abstractions.Login
{
    public interface ILoginService
    {
        Task LoginAsync(CancellationToken cancellationToken = default);
    }
}
