using System.Text.Json.Serialization;

namespace Application.PixVerse
{
    public sealed class PixVerseImageToVideoRequest
    {
        [JsonPropertyName("duration")]
        public required int Duration { get; init; }

        // Single image
        [JsonPropertyName("img_id")]
        public required long ImgId { get; init; }

        // Multi-image templates (optional)
        [JsonPropertyName("img_ids")]
        public long[]? ImgIds { get; init; }

        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("prompt")]
        public required string Prompt { get; init; }

        [JsonPropertyName("quality")]
        public required string Quality { get; init; }

        [JsonPropertyName("negative_prompt")]
        public string? NegativePrompt { get; init; }

        [JsonPropertyName("motion_mode")]
        public string? MotionMode { get; init; }

        [JsonPropertyName("style")]
        public string? Style { get; init; }

        [JsonPropertyName("template_id")]
        public long? TemplateId { get; init; }

        [JsonPropertyName("sound_effect_switch")]
        public bool? SoundEffectSwitch { get; init; }

        [JsonPropertyName("sound_effect_content")]
        public string? SoundEffectContent { get; init; }

        [JsonPropertyName("lip_sync_switch")]
        public bool? LipSyncSwitch { get; init; }

        [JsonPropertyName("lip_sync_tts_content")]
        public string? LipSyncTtsContent { get; init; }

        [JsonPropertyName("lip_sync_tts_speaker_id")]
        public string? LipSyncTtsSpeakerId { get; init; }

        [JsonPropertyName("generate_audio_switch")]
        public bool? GenerateAudioSwitch { get; init; }

        [JsonPropertyName("generate_multi_clip_switch")]
        public bool? GenerateMultiClipSwitch { get; init; }

        [JsonPropertyName("thinking_type")]
        public string? ThinkingType { get; init; }

        [JsonPropertyName("seed")]
        public int? Seed { get; init; }

        public void Validate()
        {
            if (Duration <= 0)
                throw new ArgumentOutOfRangeException(nameof(Duration), "Duration must be > 0.");

            if (ImgId <= 0 && (ImgIds is null || ImgIds.Length == 0))
                throw new ArgumentException("Either ImgId or ImgIds must be provided.");

            if (string.IsNullOrWhiteSpace(Model))
                throw new ArgumentException("Model cannot be null/empty.", nameof(Model));

            if (string.IsNullOrWhiteSpace(Prompt))
                throw new ArgumentException("Prompt cannot be null/empty.", nameof(Prompt));

            if (Prompt.Length > 2048)
                throw new ArgumentException("Prompt exceeds 2048 characters.", nameof(Prompt));

            if (NegativePrompt is not null && NegativePrompt.Length > 2048)
                throw new ArgumentException("NegativePrompt exceeds 2048 characters.", nameof(NegativePrompt));

            if (string.IsNullOrWhiteSpace(Quality))
                throw new ArgumentException("Quality cannot be null/empty.", nameof(Quality));

            if (Seed is < 0 or > 2147483647)
                throw new ArgumentOutOfRangeException(nameof(Seed), "Seed must be between 0 and 2147483647.");
        }
    }
}
