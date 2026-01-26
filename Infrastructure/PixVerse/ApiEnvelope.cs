using System.Text.Json.Serialization;

namespace Infrastructure.PixVerse;

internal sealed class ApiEnvelope<T>
{
    [JsonPropertyName("ErrCode")]
    public int ErrCode { get; init; }

    [JsonPropertyName("ErrMsg")]
    public string? ErrMsg { get; init; }

    [JsonPropertyName("Resp")]
    public T? Resp { get; init; }
}

