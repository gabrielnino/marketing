using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;

namespace Services.UrlValidation
{
    public sealed class InstagramUrlValidator(HttpClient httpClient, IOptionsMonitor<UrlOptions> options) : HttpValidatorBase(httpClient, options)
    {
        public override UrlPlatform Platform => UrlPlatform.Instagram;

        protected override PlatformRules Rules(UrlOptions opt) => opt.Instagram;
    }

}
