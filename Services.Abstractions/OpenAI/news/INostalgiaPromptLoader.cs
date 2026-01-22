using Domain.WhatsApp;

namespace Services.Abstractions.OpenAI.news
{
    public interface INostalgiaPromptLoader
    {
        Task<NostalgiaPrompt> LoadPromptAsync();
    }
}
