using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IImageClient

    {
        
        Task<Operation<UploadImage>> UploadAsync(
            Stream imageStream,
            string fileName,
            string contentType,
            CancellationToken ct = default);

        Task<Operation<UploadImage>> UploadAsync(
            string imageUrl,
            CancellationToken ct = default);
    }
}
