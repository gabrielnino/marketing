using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface ILipSyncClient
    {
        Task<Operation<JobReceipt>> SubmitAsync(
            VideoLipSync request,
            CancellationToken ct = default);

    }
}
