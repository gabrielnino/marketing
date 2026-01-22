using Services.Abstractions.UrlValidation;

namespace Services.UrlValidation
{
    public sealed class UrlValidationPipeline : IUrlValidationPipeline
    {
        private readonly IUrlAvailabilityValidatorFactory _factory;

        public UrlValidationPipeline(IUrlAvailabilityValidatorFactory factory)
        {
            _factory = factory;
        }

        public async Task<(bool AllValid, IReadOnlyList<UrlValidationResult> Results)> ValidateAllAsync(
            IReadOnlyList<string> urls,
            CancellationToken ct = default)
        {
            var results = new List<UrlValidationResult>(urls.Count);

            foreach (var url in urls)
            {
                var validator = _factory.GetValidator(url);
                var result = await validator.ValidateAsync(url, ct);
                results.Add(result);

                // "all or nothing"
                if (!result.IsValid)
                    return (false, results);
            }

            return (true, results);
        }
    }
}
