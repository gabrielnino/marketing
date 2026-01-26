namespace Application.PixVerse
{

    public sealed class GenerationStatus
    {
        public required string JobId { get; init; }

        public JobState State { get; init; } = JobState.Unknown;

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
            State is JobState.Succeeded
                or JobState.Failed
                or JobState.Cancelled;
    }
}
