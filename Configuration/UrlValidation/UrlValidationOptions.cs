using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration.UrlValidation
{
    public sealed class UrlValidationOptions
    {
        public const string SectionName = "UrlValidation";

        public int TimeoutSeconds { get; set; } = 15;
        public int MaxBodyCharsToScan { get; set; } = 200_000;

        public PlatformRules YouTube { get; set; } = new();
        public PlatformRules TikTok { get; set; } = new();
        public PlatformRules Instagram { get; set; } = new();
    }

    public sealed class PlatformRules
    {
        public List<string> MustContain { get; set; } = new();
        public List<string> MustNotContain { get; set; } = new();
    }
}
