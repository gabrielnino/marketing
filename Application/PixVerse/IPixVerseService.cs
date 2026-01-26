using Application.Result;

namespace Application.PixVerse
{
    public interface IPixVerseService
    {
        // -----------------------------
        // Account / Billing
        // -----------------------------
        Task<Operation<PixVerseBalance>> CheckBalanceAsync(CancellationToken ct = default);

        // -----------------------------
        // Text-to-Video (Async Orchestration)
        // -----------------------------
        Task<Operation<PixVerseJobSubmitted>> SubmitTextToVideoAsync(
            PixVerseTextToVideoRequest request,
            CancellationToken ct = default);

        Task<Operation<PixVerseGenerationStatus>> GetGenerationStatusAsync(
            string jobId,
            CancellationToken ct = default);

        Task<Operation<PixVerseGenerationResult>> GetGenerationResultAsync(
            string jobId,
            CancellationToken ct = default);

        /// <summary>
        /// Opinionated polling helper: polls status until terminal state,
        /// or hits max attempts / cancellation.
        /// </summary>
        Task<Operation<PixVerseGenerationResult>> WaitForCompletionAsync(
            string jobId,
            CancellationToken ct = default);
    }
}
