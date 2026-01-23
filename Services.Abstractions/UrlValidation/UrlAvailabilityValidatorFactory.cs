namespace Services.Abstractions.UrlValidation
{
    public sealed class UrlAvailabilityValidatorFactory(
        IUrlPlatformResolver resolver,
        IEnumerable<IUrlAvailabilityValidator> validators) : IUrlAvailabilityValidatorFactory
    {
        private readonly IUrlPlatformResolver _resolver = resolver;
        private readonly IReadOnlyDictionary<UrlPlatform, IUrlAvailabilityValidator> _validators = validators.ToDictionary(v => v.Platform, v => v);

        public IUrlAvailabilityValidator GetValidator(string url)
        {
            var platform = _resolver.Resolve(url);

            if (platform == UrlPlatform.Unknown || !_validators.TryGetValue(platform, out var validator))
            {
                var message = $"No validator registered for platform '{platform}'.";
                throw new NotSupportedException(message);
            }

            return validator;
        }
    }
}
