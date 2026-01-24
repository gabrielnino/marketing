using Domain;
using Prompts.NostalgiaRank;

namespace Services.Abstractions.OpenAI.news
{
    public interface INostalgiaPromptLoader
    {
        Task<NostalgiaRankPrompt> LoadPromptAsync();
    }
}
