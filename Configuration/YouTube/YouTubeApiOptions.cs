namespace Configuration.YouTube
{
    public sealed class YouTubeApiOptions
    {
        public const string SectionName = "YouTube";

        public string ApiKey { get; init; } = null!;
        public string BaseUrl { get; init; } = "https://www.googleapis.com/youtube/v3/";
    }

}
