namespace Services.Abstractions.UrlValidation
{
    public sealed class UrlPlatformResolver : IUrlPlatformResolver
    {
        public UrlPlatform Resolve(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return UrlPlatform.Unknown;

            var host = uri.Host.ToLowerInvariant();

            if (host.Contains("youtube.com") || host.Contains("youtu.be"))
                return UrlPlatform.YouTube;

            if (host.Contains("tiktok.com"))
                return UrlPlatform.TikTok;

            if (host.Contains("instagram.com"))
                return UrlPlatform.Instagram;

            return UrlPlatform.Unknown;
        }
    }
}
