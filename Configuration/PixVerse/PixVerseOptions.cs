namespace Configuration.PixVerse
{
    public sealed class PixVerseOptions
    {
        /// <summary>
        /// Base URL for PixVerse API (e.g. https://api.platform.pixverse.ai)
        /// </summary>
        public required string BaseUrl { get; init; }

        /// <summary>
        /// Static API Key for PixVerse authentication
        /// Sent as Authorization header
        /// </summary>
        public required string ApiKey { get; init; }

        /// <summary>
        /// HTTP timeout for PixVerse requests
        /// </summary>
        public TimeSpan HttpTimeout { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Delay between polling attempts when checking generation status
        /// </summary>
        public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Maximum number of polling attempts before timing out
        /// </summary>
        public int MaxPollingAttempts { get; init; } = 40;

        /// <summary>
        /// Fail fast if account balance is below this threshold
        /// </summary>
        public decimal MinimumRequiredBalance { get; init; } = 0m;
    }
}
