using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IPixVerseService
    {
        
        Task<Operation<UploadImage>> UploadImageAsync(
            Stream imageStream,
            string fileName,
            string contentType,
            CancellationToken ct = default);

        Task<Operation<UploadImage>> UploadImageAsync(
            string imageUrl,
            CancellationToken ct = default);
    }
}
