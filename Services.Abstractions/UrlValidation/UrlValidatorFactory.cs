namespace Services.Abstractions.UrlValidation
{
    public sealed class UrlValidatorFactory(
        IPlatformResolver resolver,
        IEnumerable<IUrValidator> validators) : IUrlValidatorFactory
    {
        private readonly IPlatformResolver _resolver = resolver;
        private readonly IReadOnlyDictionary<UrlPlatform, IUrValidator> _validators = validators.ToDictionary(v => v.Platform, v => v);

        public IUrValidator GetValidator(string url)
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
