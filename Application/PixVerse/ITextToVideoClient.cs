using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface ITextToVideoClient
    {
        Task<Operation<JobSubmitted>> SubmitTAsync(
            TextToVideo request,
            CancellationToken ct = default);
    }
}
