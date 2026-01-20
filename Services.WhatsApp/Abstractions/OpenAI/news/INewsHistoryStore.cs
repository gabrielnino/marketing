using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.WhatsApp.Abstractions.OpenAI.news
{
    public interface INewsHistoryStore
    {
        Task<HashSet<string>> LoadUsedUrlsAsync(CancellationToken ct = default);
        Task AppendUsedUrlsAsync(IEnumerable<string> urls, CancellationToken ct = default);
    }
}
