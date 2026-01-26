using Application.Result;

namespace Application.PixVerse
{
    public interface IPixVerseService
    {

        Task<Operation<PixVerseBalance>> CheckBalanceAsync(CancellationToken ct = default);

        Task<Operation<PixVerseJobSubmitted>> SubmitTextToVideoAsync(
            PixVerseTextToVideoRequest request,
            CancellationToken ct = default);

        Task<Operation<PixVerseUploadImageResult>> UploadImageAsync(
            Stream imageStream,
            string fileName,
            string contentType,
            CancellationToken ct = default);

        Task<Operation<PixVerseUploadImageResult>> UploadImageAsync(
            string imageUrl,
            CancellationToken ct = default);


        Task<Operation<PixVerseGenerationStatus>> GetGenerationStatusAsync(
            string jobId,
            CancellationToken ct = default);

        Task<Operation<PixVerseGenerationResult>> GetGenerationResultAsync(
            string jobId,
            CancellationToken ct = default);


        Task<Operation<PixVerseGenerationResult>> WaitForCompletionAsync(
            string jobId,
            CancellationToken ct = default);
    }
}
