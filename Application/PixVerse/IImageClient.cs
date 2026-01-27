using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IImageClient
    {
        Task<Operation<ImageResult>> UploadAsync(
            Stream imageStream,
            string fileName,
            string contentType,
            CancellationToken ct = default);

        Task<Operation<ImageResult>> UploadAsync(
            string imageUrl,
            CancellationToken ct = default);
    }
}
