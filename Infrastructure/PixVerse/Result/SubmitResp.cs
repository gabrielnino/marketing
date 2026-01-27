using System.Text.Json.Serialization;

namespace Infrastructure.PixVerse.Result;

internal sealed class SubmitResp : I2VSubmitResp
{

    [JsonPropertyName("credits")]
    public int Credits { get; init; }
}