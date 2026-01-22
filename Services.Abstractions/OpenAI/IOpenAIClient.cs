using Domain.WhatsApp;
using Domain.WhatsApp.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions.OpenAI
{
    public interface IOpenAIClient
    {
        Task<string> GetChatCompletionAsync(Prompt prompt, CancellationToken ct = default);
    }
}
