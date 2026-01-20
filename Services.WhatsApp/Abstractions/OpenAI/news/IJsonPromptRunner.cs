using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WhatsApp.Abstractions.OpenAI.news
{
    public interface IJsonPromptRunner
    {
        Task<string> RunStrictJsonAsync(string prompt, CancellationToken ct = default);
    }
}
