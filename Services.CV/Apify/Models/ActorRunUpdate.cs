using System.Text.Json.Serialization;

namespace Application.Apify.Models
{
    public class ActorRunUpdate
    {
        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; set; }

        [JsonPropertyName("isStatusMessageTerminal")]
        public bool? IsStatusMessageTerminal { get; set; }
    }
}
