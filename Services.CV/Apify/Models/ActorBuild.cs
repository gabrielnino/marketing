using System.Text.Json.Serialization;

namespace Application.Apify.Models
{
    public class ActorBuild
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("actId")]
        public string PendingActId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("startedAt")]
        public DateTime? StartedAt { get; set; }

        [JsonPropertyName("finishedAt")]
        public DateTime? FinishedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("meta")]
        public Dictionary<string, object>? Meta { get; set; }

        [JsonPropertyName("stats")]
        public Dictionary<string, object>? Stats { get; set; }
 
        [JsonPropertyName("usage")]
        public Dictionary<string, object>? Usage { get; set; }

        [JsonPropertyName("options")]
        public Dictionary<string, object>? Options { get; set; }
    }
}
