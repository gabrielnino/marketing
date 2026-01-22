using OpenQA.Selenium;
using Services.Abstractions.Selenium;

namespace Services.Selenium
{
    internal sealed class WebDriverFacade(IWebDriver driver) : IWebDriverFacade
    {
        public IReadOnlyCollection<IWebElement> FindElements(By by)
            => driver.FindElements(by);
    }
}
