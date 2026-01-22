using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.OpenAI
{
    public class OpenAIChatResponse
    {
        [JsonPropertyName("choices")]
        public required List<OpenAIChatChoice> Choices { get; set; }
    }
}
