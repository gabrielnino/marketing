using Domain.OpenAI;

namespace Services.Abstractions.OpenAI
{
    public interface IOpenAIClient
    {
        Task<string> GetChatCompletionAsync(Prompt prompt, CancellationToken ct = default);
    }
}
