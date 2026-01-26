using System.Text.Json.Serialization;

namespace Application.PixVerse.request
{
    public sealed class UploadImageResult
    {
        [JsonPropertyName("img_id")]
        public long ImgId { get; init; }

        [JsonPropertyName("img_url")]
        public string? ImgUrl { get; init; }
    }
}
