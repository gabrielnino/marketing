namespace Domain.WhatsApp
{
    public sealed class ImageMessagePayload
    {
        public string StoredImagePath { get; init; } = default!;
        public string Caption { get; init; } = string.Empty;
    }
}
