using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IImageToVideoClient
    {
        Task<Operation<JobReceipt>> SubmitAsync(
            ImageToVideo request,
            CancellationToken ct = default);
    }
}
