using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;

namespace Services
{
    public class Util: IUtil
    {
        private readonly IWebDriver _driver;
        private readonly AppConfig _config;
        private readonly ILogger<Util> _logger;
        private readonly ExecutionTracker _executionOptions;
        private const string FolderName = "Page";
        private readonly ISecurityCheck _securityCheck;
        private string FolderPath => Path.Combine(_executionOptions.ExecutionFolder, FolderName);
        private readonly ICaptureSnapshot _capture;
        private readonly IDirectoryCheck _directoryCheck;
   
        public Util(IWebDriver driver,
            AppConfig config,
            ILogger<Util> logger,
            ExecutionTracker executionOptions,
            ICaptureSnapshot capture,
            ISecurityCheck securityCheck,
            IDirectoryCheck directoryCheck)
        {
            _driver = driver;
            _config = config;
            _logger = logger;
            _executionOptions = executionOptions;
            _capture = capture;
            _securityCheck = securityCheck;
            _directoryCheck = directoryCheck;
            _directoryCheck.EnsureDirectoryExists(FolderPath);
        }

        public async Task<bool> WaitForPageLoadAsync(int timeoutInSeconds = 30)
        {
            try
            {
                string xpath = "//button[@aria-label='Next' and .//span[text()='Next']]";
                var nextButton = _driver.FindElements(By.XPath(xpath))
                                        .FirstOrDefault(b => b.Enabled);

                if (nextButton == null)
                {
                    _logger.LogInformation($"⏹️ ID:{_executionOptions.TimeStamp} No 'Next' pagination button found. Pagination completed.");
                    return false;
                }

                _logger.LogDebug($"⏭️ ID:{_executionOptions.TimeStamp} Clicking 'Next' button to go to next results page...");
                nextButton.Click();
                await Task.Delay(3000);

                if (_securityCheck.IsSecurityCheck())
                {
                    await _securityCheck.HandleSecurityPage();
                    throw new InvalidOperationException(
                        $"❌ ID:{_executionOptions.TimeStamp} LinkedIn requires manual security verification. Please complete it in the browser.");
                }

                // Verifica si la siguiente página cargó correctamente
                var container = _driver.FindElements(By.XPath("//ul[@role='list']")).FirstOrDefault();
                if (container == null)
                {
                    await _securityCheck.HandleUnexpectedPage();
                    throw new InvalidOperationException(
                        $"❌ ID:{_executionOptions.TimeStamp} Failed to load next page. Current URL: {_driver.Url}");
                }

                _logger.LogInformation($"✅ ID:{_executionOptions.TimeStamp} Successfully navigated to the next page.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"⚠️ ID:{_executionOptions.TimeStamp} Exception during next-page navigation.");
                return false;
            }
        }

        public async Task<bool> NavigateToNextPageAsync()
        {
            try
            {
                string xpath = "//button[@aria-label='Next' and .//span[text()='Next']]";
                var nextButton = _driver.FindElements(By.XPath(xpath))
                                        .FirstOrDefault(b => b.Enabled);

                if (nextButton == null)
                {
                    _logger.LogInformation($"⏹️ ID:{_executionOptions.TimeStamp} No 'Next' pagination button found. Pagination completed.");
                    return false;
                }

                _logger.LogDebug($"⏭️ ID:{_executionOptions.TimeStamp} Clicking 'Next' button to go to next results page...");
                nextButton.Click();
                await Task.Delay(3000);

                if (_securityCheck.IsSecurityCheck())
                {
                    await _securityCheck.HandleSecurityPage();
                    throw new InvalidOperationException(
                        $"❌ ID:{_executionOptions.TimeStamp} LinkedIn requires manual security verification. Please complete it in the browser.");
                }

                // Verifica si la siguiente página cargó correctamente
                var container = _driver.FindElements(By.XPath("//ul[@role='list']")).FirstOrDefault();
                if (container == null)
                {
                    await _securityCheck.HandleUnexpectedPage();
                    throw new InvalidOperationException(
                        $"❌ ID:{_executionOptions.TimeStamp} Failed to load next page. Current URL: {_driver.Url}");
                }

                _logger.LogInformation($"✅ ID:{_executionOptions.TimeStamp} Successfully navigated to the next page.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"⚠️ ID:{_executionOptions.TimeStamp} Exception during next-page navigation.");
                return false;
            }
        }

        public void ScrollMove()
        {
            var jsExecutor = (IJavaScriptExecutor)_driver;
            const int stepSize = 500; // píxeles por paso
            const int delayMs = 800;  // espera entre pasos

            long totalHeight = (long)jsExecutor.ExecuteScript("return document.body.scrollHeight");
            long currentPosition = 0;

            _logger.LogInformation("⬇️ ID:{TimeStamp} Starting step-by-step scroll on LinkedIn...", _executionOptions.TimeStamp);

            while (currentPosition < totalHeight)
            {
                jsExecutor.ExecuteScript($"window.scrollTo(0, {currentPosition});");
                Thread.Sleep(delayMs);

                currentPosition += stepSize;
                totalHeight = (long)jsExecutor.ExecuteScript("return document.body.scrollHeight"); // Actualiza por si cargó más contenido

                _logger.LogDebug("🔁 ID:{TimeStamp} Scrolled to: {CurrentPosition}/{TotalHeight}", _executionOptions.TimeStamp, currentPosition, totalHeight);
            }

            _logger.LogInformation("✅ ID:{TimeStamp} Step-by-step scroll completed.", _executionOptions.TimeStamp);
        }
        public void ScrollToTop()
        {
            var jsExecutor = (IJavaScriptExecutor)_driver;

            _logger.LogInformation("⬆️ ID:{TimeStamp} Scrolling to the top of the page...", _executionOptions.TimeStamp);

            jsExecutor.ExecuteScript("window.scrollTo(0, 0);");
            Thread.Sleep(1000); // Pequeña espera para permitir renderizado

            _logger.LogInformation("✅ ID:{TimeStamp} Scroll to top completed.", _executionOptions.TimeStamp);
        }
        public void ScrollToExperienceSection()
        {
            try
            {
                _logger.LogInformation("🔍 ID:{TimeStamp} Scrolling to the 'Experience' section...", _executionOptions.TimeStamp);

                var jsExecutor = (IJavaScriptExecutor)_driver;

                var experienceSection = _driver.FindElement(By.CssSelector("section[data-section='experience']"));

                jsExecutor.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'smooth', block: 'start' });", experienceSection);

                Thread.Sleep(1000); // Espera opcional

                _logger.LogInformation("✅ ID:{TimeStamp} Successfully scrolled to the 'Experience' section.", _executionOptions.TimeStamp);
            }
            catch (NoSuchElementException)
            {
                _logger.LogWarning("⚠️ ID:{TimeStamp} 'Experience' section not found using [data-section='experience'].", _executionOptions.TimeStamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ID:{TimeStamp} Failed to scroll to the 'Experience' section.", _executionOptions.TimeStamp);
            }
        }
    }
}
