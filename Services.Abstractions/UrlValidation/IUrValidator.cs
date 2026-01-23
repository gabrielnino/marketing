namespace Services.Abstractions.UrlValidation
{
    public interface IUrValidator
    {
        UrlPlatform Platform { get; }
        Task<UrlValidationResult> ValidateAsync(string url, CancellationToken ct = default);
    }
}
