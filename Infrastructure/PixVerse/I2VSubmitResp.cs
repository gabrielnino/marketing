using System.Text.Json.Serialization;

namespace Infrastructure.PixVerse;

internal sealed class I2VSubmitResp
{
    [JsonPropertyName("video_id")]
    public long VideoId { get; init; }
}

