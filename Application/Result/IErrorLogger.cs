namespace Application.Result
{
    public interface IErrorLogger
    {
        Task LogAsync(Exception ex, CancellationToken cancellationToken = default);
    }
}
