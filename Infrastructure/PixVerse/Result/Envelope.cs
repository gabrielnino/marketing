using System.Text.Json.Serialization;

namespace Infrastructure.PixVerse.Result;

internal sealed class Envelope<T>
{
    [JsonPropertyName("ErrCode")]
    public int ErrCode { get; init; }

    [JsonPropertyName("ErrMsg")]
    public string? ErrMsg { get; init; }

    [JsonPropertyName("Resp")]
    public T? Resp { get; init; }
}

