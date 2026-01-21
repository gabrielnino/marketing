using Domain.WhatsApp;

namespace Services.WhatsApp.Abstractions.OpenAI.news
{
    public interface INostalgiaPromptLoader
    {
        Task<NostalgiaPrompt> LoadPromptAsync();
    }
}
