using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IVideoClient
    {
        Task<Operation<FileInfo>> DownloadAsync(
            long jobId,
            string destinationFilePath,
            int videoIndex = 0,
            CancellationToken ct = default);
    }
}
