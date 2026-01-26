using System.Text.Json.Serialization;

namespace Infrastructure.PixVerse;

public sealed partial class PixVerseService
{
    private sealed class SubmitResp
    {
        [JsonPropertyName("video_id")]
        public long VideoId { get; init; }

        [JsonPropertyName("credits")]
        public int Credits { get; init; }
    }
}
