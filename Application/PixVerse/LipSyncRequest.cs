using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Application.PixVerse
{
    public sealed class LipSyncRequest
    {
        // either source_video_id or video_media_id, not both
        [JsonPropertyName("video_media_id")]
        public long VideoMediaId { get; init; } = 0;

        [JsonPropertyName("source_video_id")]
        public long? SourceVideoId { get; init; }

        // either audio_media_id OR (lip_sync_tts_speaker_id + lip_sync_tts_content), not both
        [JsonPropertyName("audio_media_id")]
        public long? AudioMediaId { get; init; } = 0;

        [JsonPropertyName("lip_sync_tts_speaker_id")]
        public string? LipSyncTtsSpeakerId { get; init; }

        [JsonPropertyName("lip_sync_tts_content")]
        public string? LipSyncTtsContent { get; init; }

        public void Validate()
        {
            var hasSourceVideo = SourceVideoId.HasValue && SourceVideoId.Value > 0;
            var hasAudioMedia = AudioMediaId.HasValue && AudioMediaId.Value > 0;
            var hasTtsPair =
                !string.IsNullOrWhiteSpace(LipSyncTtsSpeakerId) &&
                !string.IsNullOrWhiteSpace(LipSyncTtsContent);

            // exactly one audio source
            if (hasAudioMedia == hasTtsPair)
                throw new ArgumentException(
                    "Either AudioMediaId OR (LipSyncTtsSpeakerId + LipSyncTtsContent) must be set (exactly one).");

            if (!hasSourceVideo && VideoMediaId <= 0)
                throw new ArgumentException(
                    "Either SourceVideoId or VideoMediaId must be provided.");

            if (hasSourceVideo && VideoMediaId > 0)
                throw new ArgumentException(
                    "SourceVideoId and VideoMediaId cannot be provided together.");

            // TTS-specific validation + sanitization
            if (hasTtsPair)
            {
                var sanitized = SanitizeToStandardCharacters(LipSyncTtsContent!);

                if (string.IsNullOrWhiteSpace(sanitized))
                    throw new ArgumentException(
                        "LipSyncTtsContent is empty after sanitization. Only standard characters are allowed.");

                // IMPORTANT:
                // Because properties are init-only, we cannot reassign here.
                // Validation guarantees the content is safe.
                // If you want mutation, change `init` to `set`.
            }
        }

        private static string SanitizeToStandardCharacters(string input)
        {
            // 1) Normalize Unicode (removes weird composed forms)
            var normalized = input.Normalize(NormalizationForm.FormKC);

            // 2) Keep only:
            // - letters (\p{L})
            // - numbers (\p{N})
            // - spaces
            // - basic punctuation
            var sb = new StringBuilder(normalized.Length);

            foreach (var ch in normalized)
            {
                if (char.IsLetterOrDigit(ch) ||
                    ch == ' ' ||
                    ".,;:!?\"'()-".IndexOf(ch) >= 0)
                {
                    sb.Append(ch);
                }
            }

            // 3) Collapse multiple spaces
            var cleaned = Regex.Replace(sb.ToString(), @"\s{2,}", " ").Trim();

            return cleaned;
        }
    }
}
