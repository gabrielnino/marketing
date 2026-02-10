using System.Text.Json.Serialization;

namespace Application.Apify.Models
{
    public class PaginatedList<T>
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("desc")]
        public bool Desc { get; set; }

        [JsonPropertyName("items")]
        public List<T> Items { get; set; } = [];
    }
}
