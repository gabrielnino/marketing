namespace Services.Abstractions.UrlValidation
{
    public sealed record UrlValidationResult(
        bool IsValid,
        UrlPlatform Platform,
        int? HttpStatusCode,
        string? FailureReason,
        string? EvidenceSnippet = null
    );
}
