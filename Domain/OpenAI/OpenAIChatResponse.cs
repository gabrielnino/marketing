using System.Text.Json.Serialization;

namespace Domain.OpenAI
{
    public class OpenAIChatResponse
    {
        [JsonPropertyName("choices")]
        public required List<OpenAIChatChoice> Choices { get; set; }
    }
}
