namespace Domain.OpenAI
{
    using System.Text.Json.Serialization;

    public class OpenAIChatChoice
    {
        [JsonPropertyName("message")]
        public required OpenAIMessage Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public required string FinishReason { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }
}
