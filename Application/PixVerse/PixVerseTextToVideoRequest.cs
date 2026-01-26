using System.Text.Json.Serialization;

namespace Application.PixVerse
{
    public sealed class PixVerseTextToVideoRequest
    {
        /// <summary>
        /// PixVerse model (e.g. "v3.5", "v4", "v4.5" depending on API support)
        /// </summary>
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        /// <summary>
        /// Main prompt (<= 2048 characters)
        /// </summary>
        [JsonPropertyName("prompt")]
        public required string Prompt { get; init; }

        /// <summary>
        /// Optional negative prompt (<= 2048 characters)
        /// </summary>
        [JsonPropertyName("negative_prompt")]
        public string? NegativePrompt { get; init; }

        /// <summary>
        /// Optional camera movement string (supported values depend on model)
        /// </summary>
        [JsonPropertyName("camera_movement")]
        public string? CameraMovement { get; init; }

        /// <summary>
        /// Optional style (e.g. "anime", "3d_animation", "day", "cyberpunk", "comic")
        /// Do not include unless needed.
        /// </summary>
        [JsonPropertyName("style")]
        public string? Style { get; init; }

        /// <summary>
        /// Optional motion mode ("normal" default, "fast" only allows 5s, 1080p doesn't support fast)
        /// </summary>
        [JsonPropertyName("motion_mode")]
        public string? MotionMode { get; init; }

        /// <summary>
        /// Required duration (5 or 8). 1080p doesn't support 8.
        /// </summary>
        [JsonPropertyName("duration")]
        public required int Duration { get; init; }

        /// <summary>
        /// Required quality (e.g. "540p", "720p", "1080p"; "360p" for Turbo)
        /// </summary>
        [JsonPropertyName("quality")]
        public required string Quality { get; init; }

        /// <summary>
        /// Optional random seed (0..2147483647)
        /// </summary>
        [JsonPropertyName("seed")]
        public int? Seed { get; init; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Model))
                throw new ArgumentException("Model cannot be null/empty.", nameof(Model));

            if (string.IsNullOrWhiteSpace(Prompt))
                throw new ArgumentException("Prompt cannot be null/empty.", nameof(Prompt));

            if (Prompt.Length > 2048)
                throw new ArgumentException("Prompt exceeds 2048 characters.", nameof(Prompt));

            if (NegativePrompt is not null && NegativePrompt.Length > 2048)
                throw new ArgumentException("NegativePrompt exceeds 2048 characters.", nameof(NegativePrompt));

            if (Duration is not (5 or 8))
                throw new ArgumentOutOfRangeException(nameof(Duration), "Duration must be 5 or 8.");

            if (string.IsNullOrWhiteSpace(Quality))
                throw new ArgumentException("Quality cannot be null/empty.", nameof(Quality));

            if (Seed is < 0 or > 2147483647)
                throw new ArgumentOutOfRangeException(nameof(Seed), "Seed must be between 0 and 2147483647.");
        }
    }
}
