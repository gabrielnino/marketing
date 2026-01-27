namespace Infrastructure.PixVerse.Constants
{
    public class Api
    {

        public static IReadOnlySet<string> AllowedImageMimeTypes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        public static IReadOnlySet<string> AllowedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpeg",
            ".jpg",
            ".png",
            ".webp"
        };

        public const string BalancePath  = "/openapi/v2/account/balance";
        public const string TextToVideoPath = "/openapi/v2/video/text/generate";
        public const string ImageToVideoPath = "/openapi/v2/video/img/generate";
        public const string TransitionPath = "/openapi/v2/video/transition/generate";
        public const string StatusPath = "/openapi/v2/video/status/";
        public const string ResultPath = "/openapi/v2/video/result/";
        public const string UploadImagePath = "/openapi/v2/image/upload";
        public const string LipSyncPath = "/openapi/v2/video/lip_sync/generate";

        public const string ApiKeyHeader = "API-KEY";
        public const string TraceIdHeader = "Ai-trace-id";
    }
}