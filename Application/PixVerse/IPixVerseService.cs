using Application.PixVerse.Request;
using Application.PixVerse.Response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IPixVerseService
    {
        



        // Upload Image
        Task<Operation<UploadImage>> UploadImageAsync(
            Stream imageStream,
            string fileName,
            string contentType,
            CancellationToken ct = default);

        Task<Operation<UploadImage>> UploadImageAsync(
            string imageUrl,
            CancellationToken ct = default);

        // Image-to-Video




        // Speech (Lipsync)
        Task<Operation<JobSubmitted>> SubmitLipSyncAsync(
            LipSync request,
            CancellationToken ct = default);

        // Status / Result
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
