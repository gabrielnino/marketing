namespace Application.PixVerse
{
    public enum PixVerseJobState
    {
        Unknown = 0,
        Queued = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4,
        Cancelled = 5,
        Pending
    }

    public sealed class PixVerseGenerationStatus
    {
        public required string JobId { get; init; }

        public PixVerseJobState State { get; init; } = PixVerseJobState.Unknown;

        /// <summary>
        /// Optional progress percentage if PixVerse provides it (0..100).
        /// </summary>
        public int? ProgressPercent { get; init; }

        /// <summary>
        /// Optional vendor error code/string if failed.
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>
        /// Optional error message if failed.
        /// </summary>
        public string? ErrorMessage { get; init; }

        public DateTimeOffset CheckedAtUtc { get; init; } = DateTimeOffset.UtcNow;

        public bool IsTerminal =>
            State is PixVerseJobState.Succeeded
                or PixVerseJobState.Failed
                or PixVerseJobState.Cancelled;
    }
}
