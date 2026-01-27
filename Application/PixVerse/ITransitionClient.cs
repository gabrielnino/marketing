using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface ITransitionClient
    {
        Task<Operation<JobReceipt>> SubmitAsync(
            VideoTransition request,
            CancellationToken ct = default);
    }
}
