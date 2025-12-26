namespace Services.Interfaces
{
    public interface ILoginService
    {
        Task LoginAsync(CancellationToken cancellationToken = default);
    }
}
