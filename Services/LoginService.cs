using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;

namespace Services
{
    public class LoginService(
        AppConfig config,
        IWebDriver driver,
        ILogger<LoginService> logger,
        ICaptureSnapshot capture,
        ExecutionTracker executionOptions
            ) : ILoginService
    {
        private AppConfig Config { get; } = config;
        private IWebDriver Driver { get; } = driver;
        private ILogger<LoginService> Logger { get; } = logger;
        private ICaptureSnapshot Capture { get; } = capture;
        private ExecutionTracker ExecutionOptions { get; } = executionOptions;
        private const string FolderName = "Login";
        private string FolderPath => Path.Combine(ExecutionOptions.ExecutionRunning, FolderName);

        public async Task LoginAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogInformation(
                "🔐 ID:{TimeStamp} Starting login process...",
                ExecutionOptions.TimeStamp
            );

            var url = Config.WhatsApp.Url;

            Logger.LogInformation(
                "🌐 ID:{TimeStamp} Navigating to login URL: {Url}",
                ExecutionOptions.TimeStamp,
                url
            );

            Driver.Navigate().GoToUrl(url);

            await Capture.CaptureArtifactsAsync(FolderPath, "Login_Page_Loaded");

            Logger.LogInformation(
                "📲 ID:{TimeStamp} Please open WhatsApp on your phone and complete login (scan QR / approve). Waiting for login...",
                ExecutionOptions.TimeStamp
            );

            // Wait until the user logs in (poll every 5 seconds)
            await WaitForWhatsAppLoginAsync(
                pollInterval: Config.WhatsApp.LoginPollInterval,
                timeout: Config.WhatsApp.LoginTimeout,
                cancellationToken: cancellationToken
            );

            Logger.LogInformation(
                "✅ ID:{TimeStamp} WhatsApp Web is logged in. Continuing...",
                ExecutionOptions.TimeStamp
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
                    await Capture.CaptureArtifactsAsync(FolderPath, "Login_Success_LoggedIn");
                    return;
                }

                if (state == WhatsAppLoginState.NeedsQrScan)
                {
                    Logger.LogInformation(
                        "⏳ ID:{TimeStamp} Still not logged in. Please scan the QR / approve on phone...",
                        ExecutionOptions.TimeStamp
                    );
                }
                else
                {
                    Logger.LogInformation(
                        "⏳ ID:{TimeStamp} Waiting for WhatsApp Web to be ready... state={State}",
                        ExecutionOptions.TimeStamp,
                        state
                    );
                }

                if (DateTimeOffset.UtcNow - start > timeout)
                {
                    await Capture.CaptureArtifactsAsync(FolderPath, "Login_Timeout");
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
                if (Driver.FindElements(By.CssSelector("div[role='textbox'][contenteditable='true']")).Count > 0)
                    return WhatsAppLoginState.LoggedIn;

                if (Driver.FindElements(By.CssSelector("div[role='grid'], div[role='application']")).Count > 0)
                    return WhatsAppLoginState.LoggedIn;

                // QR/login signals
                if (Driver.FindElements(By.CssSelector("canvas")).Count > 0)
                    return WhatsAppLoginState.NeedsQrScan;

                var body = Driver.FindElements(By.TagName("body")).FirstOrDefault()?.Text ?? string.Empty;
                if (body.Contains("Log in", StringComparison.OrdinalIgnoreCase) ||
                    body.Contains("Use WhatsApp", StringComparison.OrdinalIgnoreCase))
                    return WhatsAppLoginState.NeedsQrScan;

                // If document not fully ready, treat as loading
                var ready = ((IJavaScriptExecutor)Driver).ExecuteScript("return document.readyState")?.ToString();
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
