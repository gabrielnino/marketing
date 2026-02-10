using System.Text.Json.Serialization;

namespace Application.Apify.JobScraper
{
    public class ActorRunResponse
    {
        [JsonPropertyName("data")]
        public ActorRunData? Data { get; set; }
    }

    public class ActorRunData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("actId")]
        public string ActId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("defaultDatasetId")]
        public string DefaultDatasetId { get; set; } = string.Empty;
    }
}
