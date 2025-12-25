// ===============================
// FILE: Application/Result/IErrorLogger.cs
// (Nueva abstracción neutra: Application conoce SOLO la interfaz, no repos/EF)
// ===============================
namespace Application.Result
{
    public interface IErrorLogger
    {
        Task LogAsync(Exception ex, CancellationToken cancellationToken = default);
    }
}
