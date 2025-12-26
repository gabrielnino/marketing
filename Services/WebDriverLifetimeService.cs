using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;

namespace Services
{
    public sealed class WebDriverLifetimeService : IHostedService
    {
        private readonly IWebDriver _driver;

        public WebDriverLifetimeService(IWebDriver driver)
        {
            _driver = driver;
        }

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
