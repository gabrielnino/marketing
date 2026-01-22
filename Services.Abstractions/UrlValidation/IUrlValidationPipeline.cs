using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions.UrlValidation
{
    public interface IUrlValidationPipeline
    {
        /// <summary>
        /// Validates all URLs. "All-or-nothing": if any fails, returns failure (with details).
        /// </summary>
        Task<(bool AllValid, IReadOnlyList<UrlValidationResult> Results)> ValidateAllAsync(
            IReadOnlyList<string> urls,
            CancellationToken ct = default
        );
    }
}
