using System.Text.Json.Serialization;

namespace Application.Apify.Models
{
    public class Dataset
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("modifiedAt")]
        public DateTime ModifiedAt { get; set; }

        [JsonPropertyName("accessedAt")]
        public DateTime AccessedAt { get; set; }

        [JsonPropertyName("itemCount")]
        public int ItemCount { get; set; }

        [JsonPropertyName("cleanItemCount")]
        public int CleanItemCount { get; set; }

        [JsonPropertyName("actId")]
        public string? ActId { get; set; }

        [JsonPropertyName("actRunId")]
        public string? ActRunId { get; set; }
    }
}
