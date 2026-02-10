using System.Text.Json.Serialization;

namespace Domain.OpenAI
{
    public class OpenAIChatRequest(string model)
    {
        [JsonPropertyName("model")]
        public string Model { get; } = model;

        [JsonPropertyName("messages")]
        public required List<OpenAIMessage> Messages { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;
        
    }
}
