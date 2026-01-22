using OpenQA.Selenium;

namespace Services.Abstractions.Selenium
{
    public interface IWebDriverFacade
    {
        IReadOnlyCollection<IWebElement> FindElements(By by);
    }
}
