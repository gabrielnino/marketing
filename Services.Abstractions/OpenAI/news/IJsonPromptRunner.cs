namespace Services.Abstractions.OpenAI.news
{
    public interface IJsonPromptRunner
    {
        Task<List<string>> RunStrictJsonAsync(CancellationToken ct = default);
    }
}
