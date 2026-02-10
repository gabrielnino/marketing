using System.Text.Json.Serialization;

namespace Application.Apify.Models
{
    public class ApifyResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
