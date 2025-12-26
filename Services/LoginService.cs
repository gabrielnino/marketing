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

        public LoginService(
            AppConfig config,
            IWebDriver driver,
            ILogger<LoginService> logger,
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions
            )
        {
            _config = config;
            _driver = driver;
            _logger = logger;
            _capture = capture;
            _executionOptions = executionOptions;
        }

        public async Task LoginAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "🔐 ID:{TimeStamp} Starting login process...",
                _executionOptions.TimeStamp
            );

            var url = _config.WhatsApp.Url;

            _logger.LogInformation(
                "🌐 ID:{TimeStamp} Navigating to login URL: {Url}",
                _executionOptions.TimeStamp,
                url
            );

            _driver.Navigate().GoToUrl(url);

            await _capture.CaptureArtifactsAsync(FolderPath, "Login_Page_Loaded");

            _logger.LogInformation(
                "📲 ID:{TimeStamp} Please open WhatsApp on your phone and complete login (scan QR / approve). Waiting for login...",
                _executionOptions.TimeStamp
            );

            // Wait until the user logs in (poll every 5 seconds)
            await WaitForWhatsAppLoginAsync(
                pollInterval: _config.WhatsApp.LoginPollInterval,
                timeout: _config.WhatsApp.LoginTimeout,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "✅ ID:{TimeStamp} WhatsApp Web is logged in. Continuing...",
                _executionOptions.TimeStamp
            );

            await Task.CompletedTask;
        }

        private async Task WaitForWhatsAppLoginAsync(
            TimeSpan pollInterval,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var start = DateTimeOffset.UtcNow;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var state = GetWhatsAppLoginState();

                if (state == WhatsAppLoginState.LoggedIn)
                {
                    await _capture.CaptureArtifactsAsync(FolderPath, "Login_Success_LoggedIn");
                    return;
                }

                if (state == WhatsAppLoginState.NeedsQrScan)
                {
                    _logger.LogInformation(
                        "⏳ ID:{TimeStamp} Still not logged in. Please scan the QR / approve on phone...",
                        _executionOptions.TimeStamp
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "⏳ ID:{TimeStamp} Waiting for WhatsApp Web to be ready... state={State}",
                        _executionOptions.TimeStamp,
                        state
                    );
                }

                if (DateTimeOffset.UtcNow - start > timeout)
                {
                    await _capture.CaptureArtifactsAsync(FolderPath, "Login_Timeout");
                    throw new TimeoutException($"WhatsApp login not completed within {timeout.TotalSeconds:N0} seconds.");
                }

                await Task.Delay(pollInterval, cancellationToken);
            }
        }

        private enum WhatsAppLoginState
        {
            LoggedIn,
            NeedsQrScan,
            Loading,
            Unknown
        }

        private WhatsAppLoginState GetWhatsAppLoginState()
        {
            try
            {
                // Logged-in signals (use multiple fallbacks)
                if (_driver.FindElements(By.CssSelector("div[role='textbox'][contenteditable='true']")).Count > 0)
                    return WhatsAppLoginState.LoggedIn;

                if (_driver.FindElements(By.CssSelector("div[role='grid'], div[role='application']")).Count > 0)
                    return WhatsAppLoginState.LoggedIn;

                // QR/login signals
                if (_driver.FindElements(By.CssSelector("canvas")).Count > 0)
                    return WhatsAppLoginState.NeedsQrScan;

                var body = _driver.FindElements(By.TagName("body")).FirstOrDefault()?.Text ?? string.Empty;
                if (body.Contains("Log in", StringComparison.OrdinalIgnoreCase) ||
                    body.Contains("Use WhatsApp", StringComparison.OrdinalIgnoreCase))
                    return WhatsAppLoginState.NeedsQrScan;

                // If document not fully ready, treat as loading
                var ready = ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState")?.ToString();
                if (!string.Equals(ready, "complete", StringComparison.OrdinalIgnoreCase))
                    return WhatsAppLoginState.Loading;

                return WhatsAppLoginState.Unknown;
            }
            catch
            {
                return WhatsAppLoginState.Unknown;
            }
        }

    }
}
