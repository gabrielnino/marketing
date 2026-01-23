namespace Services.Abstractions.UrlValidation
{
    public interface IValidationPipeline
    {
        /// <summary>
        /// Validates all URLs. "All-or-nothing": if any fails, returns failure (with details).
        /// </summary>
        Task<(bool AllValid, IReadOnlyList<UrlValidationResult> Results)> ValidateAllAsync(
            IReadOnlyList<string> urls,
            CancellationToken ct = default
        );
    }
}
