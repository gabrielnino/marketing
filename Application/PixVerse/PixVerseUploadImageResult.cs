using System.Text.Json.Serialization;

namespace Application.PixVerse
{
    public sealed class PixVerseUploadImageResult
    {
        [JsonPropertyName("img_id")]
        public long ImgId { get; init; }

        [JsonPropertyName("img_url")]
        public string? ImgUrl { get; init; }
    }
}
