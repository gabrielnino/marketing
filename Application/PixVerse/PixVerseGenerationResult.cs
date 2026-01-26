using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.PixVerse
{
    public sealed class PixVerseGenerationResult
    {
        // -----------------------------
        // Raw PixVerse fields
        // -----------------------------

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

        // -----------------------------
        // Normalized / domain-facing API
        // -----------------------------

        [JsonIgnore]
        public string JobId => RawJobId.ToString();

        [JsonIgnore]
        public PixVerseJobState State => RawStatus switch
        {
            0 => PixVerseJobState.Pending,
            1 => PixVerseJobState.Queued,
            2 => PixVerseJobState.Processing,
            5 => PixVerseJobState.Succeeded,
            6 => PixVerseJobState.Failed,
            _ => PixVerseJobState.Unknown
        };

        [JsonIgnore]
        public IReadOnlyList<string> VideoUrls =>
            string.IsNullOrWhiteSpace(Url)
                ? Array.Empty<string>()
                : new[] { Url };

        [JsonIgnore]
        public IReadOnlyList<string> PreviewUrls => Array.Empty<string>();

        [JsonIgnore]
        public string? ErrorCode => State == PixVerseJobState.Failed ? RawStatus.ToString() : null;

        [JsonIgnore]
        public string? ErrorMessage => State == PixVerseJobState.Failed ? "PixVerse generation failed" : null;

        [JsonIgnore]
        public DateTimeOffset RetrievedAtUtc { get; init; } = DateTimeOffset.UtcNow;

        [JsonIgnore]
        public bool HasAnyVideoUrl => !string.IsNullOrWhiteSpace(Url);
    }
}
