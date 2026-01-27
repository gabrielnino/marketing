namespace Application.PixVerse.Response
{ 
    public sealed class JobSubmitted
    {
        /// <summary>
        /// PixVerse generation job identifier.
        /// </summary>
        public required long JobId { get; init; }

        /// <summary>
        /// Optional server message if PixVerse returns it.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// When the job submission was recorded (UTC).
        /// </summary>
        public DateTimeOffset SubmittedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    }
}
