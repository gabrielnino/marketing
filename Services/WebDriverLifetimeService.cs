using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;

namespace Services
{
    public sealed class WebDriverLifetimeService(IWebDriver driver) : IHostedService
    {
        private readonly IWebDriver _driver = driver;

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _driver.Quit();
                _driver.Dispose();
            }
            catch
            {
                // swallow – app is shutting down
            }

            return Task.CompletedTask;
        }
    }

}
