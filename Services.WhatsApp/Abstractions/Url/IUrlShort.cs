namespace Services.WhatsApp.Abstractions.Url
{
    public interface IUrlShort
    {
        Task ShortenUrlAsync(string longUrl, string key, CancellationToken ct = default);
    }
}
