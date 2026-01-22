using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
