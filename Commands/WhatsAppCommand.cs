using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Services;
using Services.Interfaces;

namespace Commands
{
    public class WhatsAppCommand : ICommand
    {
        private readonly ILogger<WhatsAppCommand> _logger;
        private readonly IWhatsAppMessage _iWhatsAppMessage;

        public WhatsAppCommand(ILogger<WhatsAppCommand> logger, IWhatsAppMessage iWhatsAppMessage)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iWhatsAppMessage = iWhatsAppMessage ?? throw new ArgumentNullException(nameof(WhatsAppCommand));
        }

        public async Task ExecuteAsync(Dictionary<string, string>? arguments = null)
        {
            _logger.LogInformation("InviteCommand: starting. args={@Args}", arguments);
            await _iWhatsAppMessage.SendMessageAsync();
            _logger.LogInformation("InviteCommand: finished.");
        }
    }
}
