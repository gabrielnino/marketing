using System.Text.RegularExpressions;
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
        private string FolderPath => Path.Combine(_executionOptions.ExecutionRunning, FolderName);
        private readonly ISecurityCheck _securityCheck;
        private readonly IDirectoryCheck _directoryCheck;
        public LoginService(
            AppConfig config,
            IWebDriver driver,
            ILogger<LoginService> logger,
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions,
            ISecurityCheck securityCheck,
            IDirectoryCheck directoryCheck
            )
        {
            _config = config;
            _driver = driver;
            _logger = logger;
            _capture = capture;
            _executionOptions = executionOptions;
            _securityCheck = securityCheck;
            _directoryCheck = directoryCheck;
            _directoryCheck.EnsureDirectoryExists(FolderPath);
        }

        public async Task LoginAsync()
        {
            _logger.LogInformation(
                "🔐 ID:{TimeStamp} Starting login process...",
                _executionOptions.TimeStamp
            );

            var url = _config.WhatsApp.URL;

            _logger.LogInformation(
                "🌐 ID:{TimeStamp} Navigating to login URL: {Url}",
                _executionOptions.TimeStamp,
                url
            );

            _driver.Navigate().GoToUrl(url);

            await _capture.CaptureArtifactsAsync(FolderPath, "Login_Page_Loaded");


            _logger.LogInformation(
                "✅ ID:{TimeStamp} Login page loaded successfully.",
                _executionOptions.TimeStamp
            );

            _executionOptions.FinalizeByCopyThenDelete(true);
            await Task.CompletedTask;
        }
    }
}
