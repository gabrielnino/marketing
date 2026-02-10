using System.Text.Json.Serialization;

namespace Application.Apify.JobScraper
{
    public class JobScraperInput
    {
        [JsonPropertyName("site_names")]
        public List<string> SiteNames { get; set; } = [];

        [JsonPropertyName("search_term")]
        public string SearchTerm { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;
    }
}
