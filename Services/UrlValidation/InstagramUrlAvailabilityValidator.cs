using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;

namespace Services.UrlValidation
{
    public sealed class InstagramUrlAvailabilityValidator : HttpBodyRuleValidatorBase
    {
        public InstagramUrlAvailabilityValidator(HttpClient httpClient, IOptionsMonitor<UrlValidationOptions> options)
            : base(httpClient, options) { }

        public override UrlPlatform Platform => UrlPlatform.Instagram;

        protected override PlatformRules Rules(UrlValidationOptions opt) => opt.Instagram;
    }

}
