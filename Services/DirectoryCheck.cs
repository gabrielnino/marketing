using Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services
{
    public class DirectoryCheck : IDirectoryCheck
    {
        private readonly ILogger<DirectoryCheck> _logger;
        private readonly ExecutionTracker _executionOptions;
        public DirectoryCheck(ILogger<DirectoryCheck> logger, ExecutionTracker executionOptions)
        {
            _logger = logger;
            _executionOptions = executionOptions;
        }

        public void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogInformation($"📁 Created execution folder at: {_executionOptions.ExecutionFolder}");
            }
        }
    }
}
