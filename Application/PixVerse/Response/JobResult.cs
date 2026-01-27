using System.Text.Json.Serialization;

namespace Application.PixVerse.Response
{
    public sealed class JobResult
    {

        [JsonPropertyName("id")]
        public long RawJobId { get; init; }

        [JsonPropertyName("status")]
        public int RawStatus { get; init; }

        [JsonPropertyName("url")]
        public string? Url { get; init; }

        [JsonPropertyName("prompt")]
        public string? Prompt { get; init; }

        [JsonPropertyName("negative_prompt")]
        public string? NegativePrompt { get; init; }

        [JsonPropertyName("seed")]
        public int? Seed { get; init; }

        [JsonPropertyName("size")]
        public int? DurationSeconds { get; init; }

        [JsonPropertyName("has_audio")]
        public bool? HasAudio { get; init; }

        [JsonPropertyName("credits")]
        public int? Credits { get; init; }

        [JsonPropertyName("create_time")]
        public DateTimeOffset? CreateTimeUtc { get; init; }

        [JsonPropertyName("modify_time")]
        public DateTimeOffset? ModifyTimeUtc { get; init; }



        [JsonIgnore]
        public long JobId => RawJobId;

        [JsonIgnore]
        public JobState State => RawStatus switch
        {
            0 => JobState.Pending,
            1 => JobState.Queued,
            2 => JobState.Processing,
            5 => JobState.Succeeded,
            6 => JobState.Failed,
            _ => JobState.Unknown
        };

        [JsonIgnore]
        public IReadOnlyList<string> VideoUrls =>
            string.IsNullOrWhiteSpace(Url)
                ? []
                : new[] { Url };

        [JsonIgnore]
        public IReadOnlyList<string> PreviewUrls => Array.Empty<string>();

        [JsonIgnore]
        public string? ErrorCode => State == JobState.Failed ? RawStatus.ToString() : null;

        [JsonIgnore]
        public string? ErrorMessage => State == JobState.Failed ? "PixVerse generation failed" : null;

        [JsonIgnore]
        public DateTimeOffset RetrievedAtUtc { get; init; } = DateTimeOffset.UtcNow;

        [JsonIgnore]
        public bool HasAnyVideoUrl => !string.IsNullOrWhiteSpace(Url);
    }
}
