namespace Application.PixVerse
{
    public sealed class PixVerseGenerationResult
    {
        public required string JobId { get; init; }

        /// <summary>
        /// Final job state (should typically be Succeeded for a valid result).
        /// </summary>
        public PixVerseJobState State { get; init; } = PixVerseJobState.Unknown;

        /// <summary>
        /// One or more output URLs (some providers return multiple renditions).
        /// </summary>
        public IReadOnlyList<string> VideoUrls { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Optional: thumbnail / preview URLs if available.
        /// </summary>
        public IReadOnlyList<string> PreviewUrls { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Optional vendor error code/message if not successful.
        /// </summary>
        public string? ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }

        public DateTimeOffset RetrievedAtUtc { get; init; } = DateTimeOffset.UtcNow;

        public bool HasAnyVideoUrl => VideoUrls is { Count: > 0 };
    }
}
