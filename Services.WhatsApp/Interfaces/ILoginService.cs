namespace Services.WhatsApp.Interfaces
{
    public interface ILoginService
    {
        Task LoginAsync(CancellationToken cancellationToken = default);
    }
}
