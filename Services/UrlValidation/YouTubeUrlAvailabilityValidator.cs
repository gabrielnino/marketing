using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;

namespace Services.UrlValidation
{
    public sealed class YouTubeUrlAvailabilityValidator : HttpBodyRuleValidatorBase
    {
        public YouTubeUrlAvailabilityValidator(HttpClient httpClient, IOptionsMonitor<UrlValidationOptions> options)
            : base(httpClient, options) { }

        public override UrlPlatform Platform => UrlPlatform.YouTube;

        protected override PlatformRules Rules(UrlValidationOptions opt) => opt.YouTube;
    }
}
