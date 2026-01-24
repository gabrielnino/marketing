namespace Configuration.YouTube
{
    public sealed class YouTubeCurationRunnerOptions
    {
        public const string SectionName = "YouTubeCurationRunner";

        public string Query { get; init; } = "Colombia 2014 world cup highlights James Rodriguez";
        public SearchOptions Search { get; init; } = new();
    }

    public sealed class SearchOptions
    {
        public int MaxResults { get; init; } = 25;

        // Filtros típicos de YouTube Search API
        public string? RegionCode { get; init; } = "CA";         // "CA"
        public string? RelevanceLanguage { get; init; } = "es";   // "en"
        public string? Order { get; init; } = "viewCount";              // "viewCount" | "relevance" | "date" | ...
        public string? PublishedAfterIso { get; init; }   // DateTimeOffset.UtcNow.AddDays(-7).ToString("o")
        public string? PublishedBeforeIso { get; init; }
        public string? SafeSearch { get; init; }          // "none" | "moderate" | "strict"
    }
}
