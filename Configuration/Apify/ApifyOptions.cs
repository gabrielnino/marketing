namespace Configuration.Apify
{
    public class ApifyOptions
    {
        public required string ApiKey { get; set; }
        public string BaseUrl { get; set; } = "https://api.apify.com/v2";
        public string ActorId { get; set; }
    }
}
