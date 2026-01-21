using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.WhatsApp.OpenAI
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
