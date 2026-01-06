using Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services
{
    public class DirectoryCheck(ILogger<DirectoryCheck> logger, ExecutionTracker executionOptions) : IDirectoryCheck
    {
        private readonly ILogger<DirectoryCheck> _logger = logger;
        private readonly ExecutionTracker _executionOptions = executionOptions;

        public void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogInformation($"📁 Created execution folder at: {_executionOptions.ExecutionRunning}");
            }
        }
    }
}
