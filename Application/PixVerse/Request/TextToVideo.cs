using System.Text.Json.Serialization;

namespace Application.PixVerse.Request
{
    public sealed class TextToVideo
    {
        /// <summary>
        /// Aspect ratio (e.g. "16:9", "9:16", "1:1")
        /// </summary>
        [JsonPropertyName("aspect_ratio")]
        public required string AspectRatio { get; init; }

        /// <summary>
        /// Required duration (5 or 8). 1080p doesn't support 8.
        /// </summary>
        [JsonPropertyName("duration")]
        public required int Duration { get; init; }

        /// <summary>
        /// PixVerse model (e.g. "v5")
        /// </summary>
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        /// <summary>
        /// Optional negative prompt (<= 2048 characters)
        /// </summary>
        [JsonPropertyName("negative_prompt")]
        public string? NegativePrompt { get; init; }

        /// <summary>
        /// Main prompt (<= 2048 characters)
        /// </summary>
        [JsonPropertyName("prompt")]
        public required string Prompt { get; init; }

        /// <summary>
        /// Required quality (e.g. "540p", "720p", "1080p")
        /// </summary>
        [JsonPropertyName("quality")]
        public required string Quality { get; init; }

        /// <summary>
        /// Optional random seed (0..2147483647)
        /// </summary>
        [JsonPropertyName("seed")]
        public int? Seed { get; init; }

        // ---- Optional extra fields (still compatible with PixVerse UI doc) ----

        [JsonPropertyName("camera_movement")]
        public string? CameraMovement { get; init; }

        [JsonPropertyName("style")]
        public string? Style { get; init; }

        [JsonPropertyName("motion_mode")]
        public string? MotionMode { get; init; }

        [JsonPropertyName("template_id")]
        public long? TemplateId { get; init; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(AspectRatio))
                throw new ArgumentException("AspectRatio cannot be null/empty.", nameof(AspectRatio));

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
