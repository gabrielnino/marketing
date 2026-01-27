using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface ILipSyncClient
    {
        Task<Operation<JobSubmitted>> SubmitJobAsync(
          LipSync request,
          CancellationToken ct = default);

    }
}
