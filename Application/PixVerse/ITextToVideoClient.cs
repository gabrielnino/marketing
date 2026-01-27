using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface ITextToVideoClient
    {
        Task<Operation<JobReceipt>> SubmitTAsync(
            TextToVideo request,
            CancellationToken ct = default);
    }
}
