namespace Services.Abstractions.UrlValidation
{
    public interface IPlatformResolver
    {
        UrlPlatform Resolve(string url);
    }
}
