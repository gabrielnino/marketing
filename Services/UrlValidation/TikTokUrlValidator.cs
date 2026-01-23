using Configuration.UrlValidation;
using Microsoft.Extensions.Options;
using Services.Abstractions.UrlValidation;

namespace Services.UrlValidation
{
    public sealed class TikTokUrlValidator(HttpClient httpClient, IOptionsMonitor<UrlOptions> options) : HttpValidatorBase(httpClient, options)
    {
        public override UrlPlatform Platform => UrlPlatform.TikTok;

        protected override PlatformRules Rules(UrlOptions opt) => opt.TikTok;
    }
}
