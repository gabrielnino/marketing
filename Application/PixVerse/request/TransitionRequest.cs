using System;
using System.Text.Json.Serialization;

namespace Application.PixVerse.request
{
    public sealed class TransitionRequest
    {
        // -----------------------------
        // Compatibility aliases (FIX)
        // -----------------------------

        [JsonIgnore]
        public long FromImgId
        {
            init => FirstFrameImg = value;
        }

        [JsonIgnore]
        public long ToImgId
        {
            init => LastFrameImg = value;
        }

        // -----------------------------
        // PixVerse API contract
        // -----------------------------

        [JsonPropertyName("prompt")]
        public required string Prompt { get; init; }

        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("duration")]
        public required int Duration { get; init; }

        [JsonPropertyName("quality")]
        public required string Quality { get; init; }

        [JsonPropertyName("motion_mode")]
        public string? MotionMode { get; init; }

        [JsonPropertyName("seed")]
        public int? Seed { get; init; }

        [JsonPropertyName("first_frame_img")]
        public required long FirstFrameImg { get; init; }

        [JsonPropertyName("last_frame_img")]
        public required long LastFrameImg { get; init; }

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

        // -----------------------------
        // Validation
        // -----------------------------
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Prompt))
                throw new ArgumentException("Prompt cannot be null/empty.", nameof(Prompt));

            if (Prompt.Length > 2048)
                throw new ArgumentException("Prompt exceeds 2048 characters.", nameof(Prompt));

            if (string.IsNullOrWhiteSpace(Model))
                throw new ArgumentException("Model cannot be null/empty.", nameof(Model));

            if (Duration <= 0)
                throw new ArgumentOutOfRangeException(nameof(Duration), "Duration must be > 0.");

            if (string.IsNullOrWhiteSpace(Quality))
                throw new ArgumentException("Quality cannot be null/empty.", nameof(Quality));

            if (FirstFrameImg <= 0)
                throw new ArgumentOutOfRangeException(nameof(FirstFrameImg), "FirstFrameImg must be > 0.");

            if (LastFrameImg <= 0)
                throw new ArgumentOutOfRangeException(nameof(LastFrameImg), "LastFrameImg must be > 0.");

            if (Seed is < 0 or > 2147483647)
                throw new ArgumentOutOfRangeException(nameof(Seed), "Seed must be between 0 and 2147483647.");
        }
    }
}
