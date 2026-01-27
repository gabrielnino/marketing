using System.Text.Json.Serialization;

namespace Infrastructure.PixVerse.Result;

internal class I2VSubmitResp
{
    [JsonPropertyName("video_id")]
    public long VideoId { get; init; }
}

