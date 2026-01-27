using Configuration.PixVerse;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.PixVerse
{
    public class BaseVerseService(PixVerseOptions opt)
    {
        protected VerseApiSupport Helper { get; } = new VerseApiSupport(opt, JsonOpts);
        public static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
    }
}