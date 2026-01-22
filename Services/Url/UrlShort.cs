using Application.TrackedLinks;
using Services.Abstractions.Url;
using System.Text;

namespace Services.Url
{
    public class UrlShort(ITrackedLink trackedLink) : IUrlShort
    {
        private readonly ITrackedLink _trackedLink = trackedLink;
        public async Task ShortenUrlAsync(
            string longUrl,
            string key,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(longUrl))
                throw new ArgumentException("longUrl is required.", nameof(longUrl));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key is required.", nameof(key));

            await _trackedLink.UpsertAsync(id: key,targetUrl: longUrl, ct: ct);

        }
    }
}
