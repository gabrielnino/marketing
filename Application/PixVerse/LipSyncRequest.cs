using System.Text.Json.Serialization;

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

            if (hasAudioMedia == hasTtsPair) // both true OR both false
                throw new ArgumentException("Either AudioMediaId OR (LipSyncTtsSpeakerId + LipSyncTtsContent) must be set (exactly one).");

            // If user provides one of the TTS fields, enforce both.
            if (!string.IsNullOrWhiteSpace(LipSyncTtsSpeakerId) && string.IsNullOrWhiteSpace(LipSyncTtsContent))
                throw new ArgumentException("LipSyncTtsContent is required when LipSyncTtsSpeakerId is provided.");

            if (string.IsNullOrWhiteSpace(LipSyncTtsSpeakerId) && !string.IsNullOrWhiteSpace(LipSyncTtsContent))
                throw new ArgumentException("LipSyncTtsSpeakerId is required when LipSyncTtsContent is provided.");
        }
    }
}
