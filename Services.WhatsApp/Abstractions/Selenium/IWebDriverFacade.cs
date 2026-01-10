using OpenQA.Selenium;

namespace Services.WhatsApp.Abstractions.Selenium
{
    public interface IWebDriverFacade
    {
        IReadOnlyCollection<IWebElement> FindElements(By by);
    }
}
