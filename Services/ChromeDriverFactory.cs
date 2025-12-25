using System.Collections.Concurrent;
using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Services.Interfaces;

namespace Services
{
    /// <summary>
    /// Creates ChromeDriver instances with consistent options and a predictable lifecycle.
    /// IMPORTANT: This factory does NOT reuse a shared IWebDriver instance.
    /// Each Create() call returns a NEW driver. The consumer should Dispose/Quit it.
    ///
    /// If you want central disposal, this factory also tracks created drivers and will
    /// attempt to dispose them when the factory is disposed (best-effort safety net).
    /// Recommended DI lifetime: Scoped or Transient (NOT Singleton).
    /// </summary>
    public sealed class ChromeDriverFactory : IWebDriverFactory, IDisposable
    {
        private readonly ILogger<ChromeDriverFactory> _logger;
        private readonly AppConfig _appConfig;

        // One service per factory instance is fine; it will be disposed with the factory.
        private readonly ChromeDriverService _driverService;

        // Safety net: track drivers created by this factory (best-effort cleanup).
        private readonly ConcurrentBag<IWebDriver> _createdDrivers = new();

        private bool _disposed;

        public ChromeDriverFactory(ILogger<ChromeDriverFactory> logger, AppConfig appConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));

            _driverService = ChromeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;
        }

        public IWebDriver Create(bool hide = false)
        {
            ThrowIfDisposed();

            var downloadFolder = EnsureDownloadFolder();
            var options = hide
                ? GetHeadlessOptions(downloadFolder)
                : GetDefaultOptions(downloadFolder);

            return CreateDriver(options);
        }

        public IWebDriver Create(Action<ChromeOptions> configureOptions)
        {
            ThrowIfDisposed();
            if (configureOptions is null) throw new ArgumentNullException(nameof(configureOptions));

            var downloadFolder = EnsureDownloadFolder();

            // Start from defaults and let caller modify.
            var options = GetDefaultOptions(downloadFolder);
            configureOptions(options);

            return CreateDriver(options);
        }

        public ChromeOptions GetDefaultOptions(string downloadFolder)
        {
            if (string.IsNullOrWhiteSpace(downloadFolder))
                throw new ArgumentNullException(nameof(downloadFolder));

            var options = new ChromeOptions();

            options.AddArguments("--start-maximized");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            ConfigureDownloads(downloadFolder, options);

            return options;
        }

        private static ChromeOptions GetHeadlessOptions(string downloadFolder)
        {
            if (string.IsNullOrWhiteSpace(downloadFolder))
                throw new ArgumentNullException(nameof(downloadFolder));

            var options = new ChromeOptions();

            options.AddArguments("--headless=new");
            options.AddArguments("--disable-gpu");
            options.AddArguments("--window-size=1920,1080");
            options.AddArguments("--start-maximized");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            ConfigureDownloads(downloadFolder, options);

            return options;
        }

        private IWebDriver CreateDriver(ChromeOptions options)
        {
            try
            {
                _logger.LogInformation("Creating new ChromeDriver instance");
                var driver = new ChromeDriver(_driverService, options);

                SetTimeouts(driver);

                _createdDrivers.Add(driver);
                return driver;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ChromeDriver");
                throw new WebDriverException("Failed to initialize ChromeDriver", ex);
            }
        }

        private static void SetTimeouts(IWebDriver driver)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
        }

        private string EnsureDownloadFolder()
        {
            var downloadFolder = _appConfig?.Paths?.DownloadFolder;

            if (string.IsNullOrWhiteSpace(downloadFolder))
                throw new InvalidOperationException("AppConfig.Paths.DownloadFolder is missing or empty.");

            Directory.CreateDirectory(downloadFolder);
            return downloadFolder;
        }

        private static void ConfigureDownloads(string downloadFolder, ChromeOptions options)
        {
            options.AddUserProfilePreference("download.default_directory", downloadFolder);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
            options.AddUserProfilePreference("safebrowsing.enabled", true);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Best-effort cleanup: Quit+Dispose all drivers created by this factory.
            while (_createdDrivers.TryTake(out var driver))
            {
                try { driver.Quit(); } catch { /* ignore */ }
                try { driver.Dispose(); } catch { /* ignore */ }
            }

            try { _driverService.Dispose(); } catch { /* ignore */ }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ChromeDriverFactory));
            }
        }
    }
}
