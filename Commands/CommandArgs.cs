namespace Commands
{
    public class CommandArgs
    {
        public const string WhatsApp = "--whatsapp";

        private static readonly HashSet<string> ValidCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            WhatsApp
        };

        public string MainCommand { get; }
        public Dictionary<string, string> Arguments { get; }

        public CommandArgs(string[] args)
        {
            var cmd = args.FirstOrDefault(IsCommand);
            var firstArg = args.FirstOrDefault(IsArgument);
            MainCommand = cmd ?? (firstArg is null ? string.Empty : firstArg.Split('=', 2)[0]);
            Arguments = args
                .Where(IsArgument)
                .Select(arg =>
                {
                    var parts = arg.Split('=', 2);
                    var key = parts[0];
                    var value = parts.Length > 1 ? parts[1] : string.Empty;
                    return new KeyValuePair<string, string>(key, value);
                })
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsCommand(string arg) => ValidCommands.Contains(arg);

        private static bool IsArgument(string arg) => arg.Contains("=");
    }
}
