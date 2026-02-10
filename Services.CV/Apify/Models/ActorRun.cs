using System.Text.Json.Serialization;

namespace Application.Apify.Models
{
    public class ActorRun
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("actId")]
        public string ActId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("startedAt")]
        public DateTime? StartedAt { get; set; }

        [JsonPropertyName("finishedAt")]
        public DateTime? FinishedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; set; }

        [JsonPropertyName("isStatusMessageTerminal")]
        public bool? IsStatusMessageTerminal { get; set; }

        [JsonPropertyName("meta")]
        public Dictionary<string, object>? Meta { get; set; }

        [JsonPropertyName("stats")]
        public Dictionary<string, object>? Stats { get; set; }

        [JsonPropertyName("options")]
        public Dictionary<string, object>? Options { get; set; }

        [JsonPropertyName("buildId")]
        public string? BuildId { get; set; }

        [JsonPropertyName("exitCode")]
        public int? ExitCode { get; set; }

        [JsonPropertyName("defaultKeyValueStoreId")]
        public string? DefaultKeyValueStoreId { get; set; }

        [JsonPropertyName("defaultDatasetId")]
        public string? DefaultDatasetId { get; set; }

        [JsonPropertyName("defaultRequestQueueId")]
        public string? DefaultRequestQueueId { get; set; }

        [JsonPropertyName("buildNumber")]
        public string? BuildNumber { get; set; }

        [JsonPropertyName("containerUrl")]
        public string? ContainerUrl { get; set; }
        
        [JsonPropertyName("usage")]
        public Dictionary<string, object>? Usage { get; set; }
        
        [JsonPropertyName("usageTotalUsd")]
        public double? UsageTotalUsd { get; set; }
    }
}
