using System.Text.Json.Serialization;

namespace Application.PixVerse.Response
{
    public sealed class ImageResult
    {
        [JsonPropertyName("img_id")]
        public long ImgId { get; init; }

        [JsonPropertyName("img_url")]
        public string? ImgUrl { get; init; }
    }
}
