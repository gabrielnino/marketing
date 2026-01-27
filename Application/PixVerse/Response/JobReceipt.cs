namespace Application.PixVerse.Response
{ 
    public sealed class JobReceipt
    {
        public required long JobId { get; init; }
        public string? Message { get; init; }
        public DateTimeOffset SubmittedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    }
}
