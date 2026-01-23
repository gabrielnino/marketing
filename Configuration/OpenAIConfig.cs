namespace Configuration
{
    public sealed class OpenAIConfig
    {
        public string ApiKey { get; init; } = null!;
        public string UriString { get; init; } = null!;
        public string Model { get; init; } = null!;
    }
}
