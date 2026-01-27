namespace Application.PixVerse.Response
{

    public sealed class JobStatus
    {
        public required string JobId { get; init; }

        public JobState State { get; init; } = JobState.Unknown;

        public int? ProgressPercent { get; init; }

        public string? ErrorCode { get; init; }

        public string? ErrorMessage { get; init; }

        public DateTimeOffset CheckedAtUtc { get; init; } = DateTimeOffset.UtcNow;

        public bool IsTerminal =>
            State is JobState.Succeeded
                or JobState.Failed
                or JobState.Cancelled;
    }
}
