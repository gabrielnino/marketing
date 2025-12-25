using Microsoft.Extensions.DependencyInjection;


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
                //case CommandArgs.invite:
                //    commands.Add(_serviceProvider.GetRequiredService<InviteCommand>());
                //    break;
                default:
                    commands.Add(_serviceProvider.GetRequiredService<HelpCommand>());
                    break;
            }

            return commands;
        }
    }
}

