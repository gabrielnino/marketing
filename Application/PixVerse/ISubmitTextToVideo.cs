using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface ISubmitTextToVideo
    {
        Task<Operation<JobSubmitted>> SubmitTextToVideoAsync(
            TextToVideo request,
            CancellationToken ct = default);
    }
}
