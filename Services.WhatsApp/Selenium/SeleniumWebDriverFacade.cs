using OpenQA.Selenium;
using Services.WhatsApp.Abstractions.Selenium;

namespace Services.WhatsApp.Selenium
{
    internal sealed class SeleniumWebDriverFacade(IWebDriver driver) : IWebDriverFacade
    {
        public IReadOnlyCollection<IWebElement> FindElements(By by)
            => driver.FindElements(by);
    }
}
