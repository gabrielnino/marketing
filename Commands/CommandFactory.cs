using Microsoft.Extensions.DependencyInjection;
using Services.Abstractions.Check;


namespace Commands
{
    public class CommandFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandArgs _jobCommandArgs;

        public CommandFactory(IServiceProvider serviceProvider, CommandArgs jobCommandArgs)
        {
            _serviceProvider = serviceProvider;
            _jobCommandArgs = jobCommandArgs;
        }

        public IEnumerable<ICommand> CreateCommand()
        {
            var commands = new List<ICommand>();

            switch (_jobCommandArgs.MainCommand.ToLowerInvariant())
            {
                case CommandArgs.WhatsApp:
                    commands.Add(_serviceProvider.GetRequiredService<WhatsAppCommand>());
                    break;
                default:
                    commands.Add(_serviceProvider.GetRequiredService<HelpCommand>());
                    break;
            }

            return commands;
        }
    }
}

