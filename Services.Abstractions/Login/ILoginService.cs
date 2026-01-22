namespace Services.Abstractions.Login
{
    public interface ILoginService
    {
        Task LoginAsync(CancellationToken cancellationToken = default);
    }
}
