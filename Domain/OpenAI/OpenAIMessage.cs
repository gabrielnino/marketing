using System.Text.Json.Serialization;

namespace Domain.OpenAI
{
    public class OpenAIMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }
}
