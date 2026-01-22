using System.Text.Json.Serialization;

namespace Domain;
public sealed class NostalgiaPrompt
{
    [JsonPropertyName("prompt_name")]
    public string PromptName { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    public NostalgiaContext Context { get; set; } = new();

    [JsonPropertyName("task")]
    public NostalgiaTask Task { get; set; } = new();

    [JsonPropertyName("output")]
    public NostalgiaOutput Output { get; set; } = new();
}

public sealed class NostalgiaContext
{
    [JsonPropertyName("objective")]
    public string Objective { get; set; } = string.Empty;

    [JsonPropertyName("allowed_sources")]
    public List<string> AllowedSources { get; set; } = [];

    [JsonPropertyName("history")]
    public NostalgiaHistory History { get; set; } = new();

    [JsonPropertyName("excluded_urls")]
    public List<string> ExcludedUrls { get; set; } = [];
}

public sealed class NostalgiaHistory
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("rule")]
    public string Rule { get; set; } = string.Empty;
}

public sealed class NostalgiaTask
{
    [JsonPropertyName("goal")]
    public string Goal { get; set; } = string.Empty;

    [JsonPropertyName("selection_priority")]
    public List<string> SelectionPriority { get; set; } = [];

    [JsonPropertyName("mandatory_threshold")]
    public List<string> MandatoryThreshold { get; set; } = [];

    [JsonPropertyName("exclusion_rules")]
    public List<string> ExclusionRules { get; set; } = [];

    [JsonPropertyName("duration_policy")]
    public string DurationPolicy { get; set; } = string.Empty;
}

public sealed class NostalgiaOutput
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("schema")]
    public NostalgiaOutputSchema Schema { get; set; } = new();

    [JsonPropertyName("constraints")]
    public List<string> Constraints { get; set; } = [];
}

public sealed class NostalgiaOutputSchema
{
    [JsonPropertyName("urls")]
    public List<string> Urls { get; set; } = [];
}
