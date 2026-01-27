using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IGetGenerationStatus
    {
        Task<Operation<GenerationStatus>> GetGenerationStatusAsync(
           long jobId,
           CancellationToken ct = default);

        Task<Operation<Generation>> GetGenerationResultAsync(
            long jobId,
            CancellationToken ct = default);

        Task<Operation<Generation>> WaitForCompletionAsync(
            long jobId,
            CancellationToken ct = default);

        Task<Operation<FileInfo>> DownloadVideoAsync(
                 long jobId,
                 string destinationFilePath,
                 int videoIndex = 0,
                 CancellationToken ct = default);
    }
}
