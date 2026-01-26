using Application.PixVerse.request;
using Application.PixVerse.response;
using Application.Result;

namespace Application.PixVerse
{
    public interface IPixVerseService
    {
        Task<Operation<Balance>> CheckBalanceAsync(CancellationToken ct = default);

        // Text-to-Video
        Task<Operation<JobSubmitted>> SubmitTextToVideoAsync(
            TextToVideoRequest request,
            CancellationToken ct = default);

        // Upload Image
        Task<Operation<UploadImageResult>> UploadImageAsync(
            Stream imageStream,
            string fileName,
            string contentType,
            CancellationToken ct = default);

        Task<Operation<UploadImageResult>> UploadImageAsync(
            string imageUrl,
            CancellationToken ct = default);

        // Image-to-Video
        Task<Operation<JobSubmitted>> SubmitImageToVideoAsync(
            ImageToVideoRequest request,
            CancellationToken ct = default);

        // Transition (First-last frame)
        Task<Operation<JobSubmitted>> SubmitTransitionAsync(
            TransitionRequest request,
            CancellationToken ct = default);

        // Speech (Lipsync)
        Task<Operation<JobSubmitted>> SubmitLipSyncAsync(
            LipSyncRequest request,
            CancellationToken ct = default);

        // Status / Result
        Task<Operation<GenerationStatus>> GetGenerationStatusAsync(
            long jobId,
            CancellationToken ct = default);

        Task<Operation<GenerationResult>> GetGenerationResultAsync(
            long jobId,
            CancellationToken ct = default);

        Task<Operation<GenerationResult>> WaitForCompletionAsync(
            long jobId,
            CancellationToken ct = default);

        Task<Operation<FileInfo>> DownloadVideoAsync(
            long jobId,
            string destinationFilePath,
            int videoIndex = 0,
            CancellationToken ct = default);
    }
}
