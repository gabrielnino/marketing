using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;

namespace Services.UrlValidation
{
    public sealed class InstagramUrlAvailabilityValidator(HttpClient httpClient, IOptionsMonitor<UrlValidationOptions> options) : HttpBodyRuleValidatorBase(httpClient, options)
    {
        public override UrlPlatform Platform => UrlPlatform.Instagram;

        protected override PlatformRules Rules(UrlValidationOptions opt) => opt.Instagram;
    }

}
