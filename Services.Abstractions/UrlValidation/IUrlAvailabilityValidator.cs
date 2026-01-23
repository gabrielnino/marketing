namespace Services.Abstractions.UrlValidation
{
    public interface IUrlAvailabilityValidator
    {
        UrlPlatform Platform { get; }
        Task<UrlValidationResult> ValidateAsync(string url, CancellationToken ct = default);
    }
}
