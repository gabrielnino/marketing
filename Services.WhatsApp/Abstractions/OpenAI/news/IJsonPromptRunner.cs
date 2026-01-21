namespace Services.WhatsApp.Abstractions.OpenAI.news
{
    public interface IJsonPromptRunner
    {
        Task<List<string>> RunStrictJsonAsync(CancellationToken ct = default);
    }
}
