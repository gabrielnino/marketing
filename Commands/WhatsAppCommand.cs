using Microsoft.Extensions.Logging;
using Services.Abstractions.Login;

namespace Commands
{
    public class WhatsAppCommand(ILogger<WhatsAppCommand> logger, IMessage iWhatsAppMessage) : ICommand
    {
        private ILogger<WhatsAppCommand> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));
        private IMessage IWhatsAppMessage { get; } = iWhatsAppMessage ?? throw new ArgumentNullException(nameof(WhatsAppCommand));

        public async Task ExecuteAsync(Dictionary<string, string>? arguments = null)
        {
            Logger.LogInformation("InviteCommand: starting. args={@Args}", arguments);
            await IWhatsAppMessage.LoginAsync();
            await IWhatsAppMessage.SendMessageAsync();
            Logger.LogInformation("InviteCommand: finished.");
        }
    }
}
