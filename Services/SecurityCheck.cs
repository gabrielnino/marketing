using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;
using Configuration;

namespace Services
{
    public class SecurityCheck : ISecurityCheck
    {
        private readonly IWebDriver _driver;
        private readonly ICaptureSnapshot _capture;
        private readonly ExecutionTracker _executionOptions;
        private readonly ILogger<SecurityCheck> _logger;
        private const string FolderName = "SecurityCheck";
        private string FolderPath => Path.Combine(_executionOptions.ExecutionRunning, FolderName);
        private readonly IDirectoryCheck _directoryCheck;
        public SecurityCheck(IWebDriver driver,
            ILogger<SecurityCheck> logger,
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions,
            IDirectoryCheck directoryCheck)
        {
            _logger = logger;
            _capture = capture;
            _executionOptions = executionOptions;
            _directoryCheck = directoryCheck;
            _driver = driver;
            _directoryCheck.EnsureDirectoryExists(FolderPath);
        }

        public bool IsSecurityCheck()
        {
            try
            {
                var title = _driver.Title.Contains("Security Verification");
                if (title)
                {
                    _logger.LogWarning("⚠️ Title Security Verification detected on the page.");
                    return true;
                }
                var captcha = _driver.FindElements(By.Id("captcha-internal")).Any();
                if (captcha)
                {
                    _logger.LogWarning("⚠️ CAPTCHA image detected on the page.");
                    return true;
                }

                var text = _driver.FindElements(By.XPath("//h1[contains(text(), 'Let’s do a quick security check')]")).Any();

                if (text)
                {
                    _logger.LogWarning("⚠️ Text 'Let’s do a quick security check' detected on the page.");
                    return true;
                }

                // Detect common CAPTCHA indicators
                var captchaImages = _driver.FindElements(By.XPath("//img[contains(@src, 'captcha')]")).Any();
                if (captchaImages)
                {
                    _logger.LogWarning("⚠️ CAPTCHA image detected on the page.");
                    return true;
                }

                // Detect common texts indicating human check
                var bodyText = _driver.FindElement(By.TagName("body")).Text;
                var indicators = new[] { "are you a human", "please verify", "unusual activity", "security check", "confirm your identity" };

                if (indicators.Any(indicator => bodyText.IndexOf(indicator, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    _logger.LogWarning("⚠️ Security check text detected on the page.");
                    return true;
                }

                // Optionally: detect if login form re-appeared
                var loginForm = _driver.FindElements(By.XPath("//input[@name='session_key']"));
                if (loginForm.Any())
                {
                    _logger.LogWarning("⚠️ Unexpected LinkedIn login form detected. Session might have expired.");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ Error while checking for security verification.");
                return false; // Fail-safe: assume no security check if we can't verify
            }
        }

        public async Task TryStartPuzzle()
        {
            try
            {
                _logger.LogInformation("🧩 Attempting to click on 'Start Puzzle' button...");
                Console.WriteLine("🛑 Pausado. Por favor, resuelve el captcha y presiona ENTER para continuar...");
                Console.ReadLine();
                var timestampEnd = await _capture.CaptureArtifactsAsync(FolderPath, "Start_Puzzle_Clicked");
                _logger.LogInformation($"📸 Captured screenshot after clicking 'Start Puzzle' at {timestampEnd}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ID:{_executionOptions.TimeStamp} Failed to simulate click on 'Start Puzzle' button.");
            }
        }

        public async Task HandleSecurityPage()
        {
            var timestamp = await _capture.CaptureArtifactsAsync(FolderPath, "SecurityPageDetected");
            _logger.LogError($" ID:{_executionOptions.TimeStamp} Unexpected page layout detected.");
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║           SECURITY PAGE DETECTED          ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine($"║ Current URL: {_driver.Url,-30} ║");
            Console.WriteLine("║                                            ║");
            Console.WriteLine($"║ HTML saved to: {timestamp}.html ║");
            Console.WriteLine($"║ Screenshot saved to: {timestamp}.png ║");
            Console.WriteLine("╚════════════════════════════════════════════╝\n");
        }

        public async Task HandleUnexpectedPage()
        {
            var timestamp = await _capture.CaptureArtifactsAsync(FolderPath, "UnexpectedPageDetected");
            _logger.LogError($" ID:{_executionOptions.TimeStamp} Unexpected page layout detected.");
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║           UNEXPECTED PAGE DETECTED          ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine($"║ Current URL: {_driver.Url,-30} ║");
            Console.WriteLine("║                                            ║");
            Console.WriteLine($"║ HTML saved to: {timestamp}.html ║");
            Console.WriteLine($"║ Screenshot saved to: {timestamp}.png ║");
            Console.WriteLine("╚════════════════════════════════════════════╝\n");
        }

    }
}
