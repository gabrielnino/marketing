using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;

namespace Services
{
    public class LoginService : ILoginService
    {
        private readonly AppConfig _config;
        private readonly IWebDriver _driver;
        private readonly ILogger<LoginService> _logger;
        private readonly ICaptureSnapshot _capture;
        private readonly ExecutionTracker _executionOptions;
        private const string FolderName = "Login";
        private string FolderPath => Path.Combine(_executionOptions.ExecutionFolder, FolderName);
        private readonly ISecurityCheck _securityCheck;
        private readonly IDirectoryCheck _directoryCheck;
        public LoginService(
            AppConfig config, 
            IWebDriverFactory driverFactory, 
            ILogger<LoginService> logger, 
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions,
            ISecurityCheck securityCheck,
            IDirectoryCheck directoryCheck)
        {
            _config = config;
            _driver = driverFactory.Create(true);
            _logger = logger;
            _capture = capture;
            _executionOptions = executionOptions;
            _securityCheck = securityCheck;
            _directoryCheck = directoryCheck;
            _directoryCheck.EnsureDirectoryExists(FolderPath);
        }

        public async Task LoginAsync()
        {
            _logger.LogInformation($"🔐 ID:{_executionOptions.TimeStamp} Attempting to login to LinkedIn...");
            var url = _config.WhatsApp.URL;
            _driver.Navigate().GoToUrl(url);
            
        }

        
    }
}
