namespace Configuration
{
    public sealed class SchedulerOptions
    {
        public const string SectionName = "WhatsApp:Scheduler";
        public bool Enabled { get; init; } = true;
        public string TimeZoneId { get; init; } = "America/Vancouver";
        public Dictionary<string, List<string>> Weekly { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
