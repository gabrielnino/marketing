using System.Text.Json.Serialization;

namespace Application.Apify.Models
{
    public class DatasetItem
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("publishedAt")]
        public string? PublishedAt { get; set; }

        [JsonPropertyName("salary")]
        public string? Salary { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("jobUrl")]
        public string? JobUrl { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("companyUrl")]
        public string? CompanyUrl { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("postedTime")]
        public string? PostedTime { get; set; }

        [JsonPropertyName("applicationsCount")]
        public string? ApplicationsCount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("contractType")]
        public string? ContractType { get; set; }

        [JsonPropertyName("experienceLevel")]
        public string? ExperienceLevel { get; set; }

        [JsonPropertyName("workType")]
        public string? WorkType { get; set; }

        [JsonPropertyName("sector")]
        public string? Sector { get; set; }

        [JsonPropertyName("applyType")]
        public string? ApplyType { get; set; }

        [JsonPropertyName("applyUrl")]
        public string? ApplyUrl { get; set; }

        [JsonPropertyName("descriptionHtml")]
        public string? DescriptionHtml { get; set; }

        [JsonPropertyName("companyId")]
        public string? CompanyId { get; set; }

        [JsonPropertyName("benefits")]
        public string? Benefits { get; set; }

        [JsonPropertyName("posterProfileUrl")]
        public string? PosterProfileUrl { get; set; }

        [JsonPropertyName("posterFullName")]
        public string? PosterFullName { get; set; }
    }
}
