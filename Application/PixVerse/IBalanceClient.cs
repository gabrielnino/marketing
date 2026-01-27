using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IBalanceClient
    {
        Task<Operation<AccountCredits>> GetAsync(CancellationToken ct = default);
    }
}
