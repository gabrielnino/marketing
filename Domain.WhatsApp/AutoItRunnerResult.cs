namespace Domain.WhatsApp
{
    public sealed class AutoItRunnerResult
    {
        public required int ExitCode { get; init; }
        public required bool TimedOut { get; init; }
        public required string StdOut { get; init; }
        public required string StdErr { get; init; }
        public required TimeSpan Duration { get; init; }
        public string? LogFilePath { get; init; }
    }
}
