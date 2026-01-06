using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Services.Interfaces;
using System;
using System.IO;

namespace Services;

public sealed class ChromeDriverFactory(ILogger<ChromeDriverFactory> logger) : IWebDriverFactory
{
    private readonly ILogger<ChromeDriverFactory> _logger = logger;

    // ============================================================
    // IWebDriverFactory
    // ============================================================

    public IWebDriver Create(bool hide)
    {
        var options = BuildDefaultOptions(hide);
        return CreateDriver(options);
    }

    public IWebDriver Create(Action<ChromeOptions> configure)
    {
        var options = BuildDefaultOptions(hide: false);
        configure?.Invoke(options);
        return CreateDriver(options);
    }

    public ChromeOptions GetDefaultOptions(string mode)
    {
        var hide = mode.Equals("headless", StringComparison.OrdinalIgnoreCase);
        return BuildDefaultOptions(hide);
    }

    // ============================================================
    // INTERNAL
    // ============================================================

    private IWebDriver CreateDriver(ChromeOptions options)
    {
        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WhatsAppSender",
                "chromedriver-logs"
            );

            Directory.CreateDirectory(logDir);

            var logFile = Path.Combine(
                logDir,
                $"chromedriver_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Environment.ProcessId}.log"
            );

            var service = ChromeDriverService.CreateDefaultService();
            service.EnableVerboseLogging = true;
            service.LogPath = logFile;
            service.HideCommandPromptWindow = true;

            _logger.LogInformation(
                "Creating ChromeDriver (Selenium Manager). Log={LogFile}",
                logFile
            );

            var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(60));

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(60);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;

            return driver;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ChromeDriver");
            throw;
        }
    }

    private static ChromeOptions BuildDefaultOptions(bool hide)
    {
        var options = new ChromeOptions();

        // ---- Stability / automation
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-popup-blocking");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--remote-allow-origins=*");

        // ---- Headless
        if (hide)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
        }

        return options;
    }
}
