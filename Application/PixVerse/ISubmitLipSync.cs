using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface ISubmitLipSync
    {
        Task<Operation<JobSubmitted>> SubmitLipSyncAsync(
          LipSync request,
          CancellationToken ct = default);

    }
}
