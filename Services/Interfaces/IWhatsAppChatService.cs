using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IWhatsAppChatService
    {
        Task OpenContactChatAsync(string chatIdentifier, CancellationToken ct = default);
        Task SendMessageAsync(string message, CancellationToken ct = default);
    }
}
