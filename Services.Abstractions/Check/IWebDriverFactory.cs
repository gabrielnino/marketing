using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Services.Abstractions.Check
{
    public interface IWebDriverFactory
    {
        IWebDriver Create(bool hide = false);
        IWebDriver Create(Action<ChromeOptions> configureOptions);
        ChromeOptions GetDefaultOptions(string downloadFolder);
    }
}
