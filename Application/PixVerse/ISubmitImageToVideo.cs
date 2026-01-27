using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface ISubmitImageToVideo
    {
        Task<Operation<JobSubmitted>> SubmitImageToVideoAsync(
        ImageToVideo request,
        CancellationToken ct = default);
    }
}
