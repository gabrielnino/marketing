namespace Configuration
{
    public class WhatsAppConfig
    {
        public required string Url { get; set; }
        public required TimeSpan LoginPollInterval { get; init; }
        public required TimeSpan LoginTimeout { get; init; }
    }
}
