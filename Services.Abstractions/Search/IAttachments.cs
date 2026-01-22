using OpenQA.Selenium;

namespace Services.Abstractions.Search
{
    public interface IAttachments
    {
        IWebElement FindAttachButton(TimeSpan timeout, TimeSpan pollingInterval);
        IWebElement FindPhotosAndVideosOptionButton(TimeSpan timeout, TimeSpan pollingInterval);
    }
}
