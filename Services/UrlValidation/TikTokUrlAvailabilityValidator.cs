using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;

namespace Services.UrlValidation
{
    public sealed class TikTokUrlAvailabilityValidator : HttpBodyRuleValidatorBase
    {
        public TikTokUrlAvailabilityValidator(HttpClient httpClient, IOptionsMonitor<UrlValidationOptions> options)
            : base(httpClient, options) { }

        public override UrlPlatform Platform => UrlPlatform.TikTok;

        protected override PlatformRules Rules(UrlValidationOptions opt) => opt.TikTok;
    }
}
