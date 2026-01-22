using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Abstractions.Search;

namespace Services.WhatsApp.Selector
{

    public class Attachments(IWebDriver driver, ILogger<Attachments> logger) : IAttachments
    {
        private IWebDriver Driver { get; } = driver;
        public ILogger<Attachments> Logger { get; } = logger;

        private const string XpathToFindAttachButton =
      "//button[@aria-label='Attach' or @title='Attach' or @data-testid='attach-button' or .//span[@data-icon='clip' or @data-icon='plus']]";

        private const string FindPhotosAndVideosOption =
    "//*[@role='button' or @role='menuitem' or self::button]" +
    "[@aria-label='Photos & videos' or title='Photos & videos' or @data-testid='attach-photos' or .//span[normalize-space(.)='Photos & videos']]";

        public IWebElement FindAttachButton(TimeSpan timeout, TimeSpan pollingInterval)
        {
            Logger.LogDebug(
                "FindAttachButton started. timeout={Timeout} pollingInterval={PollingInterval} xpath='{XPath}'",
                timeout,
                pollingInterval,
                XpathToFindAttachButton
            );

            var wait = new WebDriverWait(Driver, timeout)
            {
                PollingInterval = pollingInterval
            };

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            IWebElement attachButton;
            try
            {
                attachButton = wait.Until(driver =>
                {
                    var element = driver
                        .FindElements(By.XPath(XpathToFindAttachButton))
                        .FirstOrDefault();

                    if (element is null)
                    {
                        Logger.LogTrace("FindAttachButton: Attach button not present yet.");
                        return null;
                    }

                    if (!element.Displayed || !element.Enabled)
                    {
                        Logger.LogTrace(
                            "FindAttachButton: Element found but not ready. displayed={Displayed}, enabled={Enabled}",
                            element.Displayed,
                            element.Enabled
                        );
                        return null;
                    }

                    return element;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                Logger.LogError(
                    ex,
                    "FindAttachButton timed out after {Timeout}. xpath='{XPath}'",
                    timeout,
                    XpathToFindAttachButton
                );
                throw;
            }

            Logger.LogDebug(
                "FindAttachButton completed. found={Found} displayed={Displayed} enabled={Enabled}",
                attachButton is not null,
                attachButton?.Displayed,
                attachButton?.Enabled
            );

            return attachButton;
        }

        public IWebElement FindPhotosAndVideosOptionButton(TimeSpan timeout, TimeSpan pollingInterval)
        {

            Logger.LogInformation(
                "Locating 'Photos & videos' option using XPath '{XPath}'...",
                FindPhotosAndVideosOption
            );
            Logger.LogDebug(
                "FindPhotosAndVideosOptionButton started. timeout={Timeout} pollingInterval={PollingInterval} xpath='{XPath}'",
                timeout, pollingInterval, FindPhotosAndVideosOption
            );

            var wait = new WebDriverWait(Driver, timeout)
            {
                PollingInterval = pollingInterval
            };

            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

            try
            {
                var result = wait.Until(driver =>
                {
                    var candidates = driver.FindElements(By.XPath(FindPhotosAndVideosOption))
                        .Where(e => e.Displayed && e.Enabled)
                        .ToList();

                    if (candidates.Count == 0)
                    {
                        Logger.LogTrace("FindPhotosAndVideosOptionButton: no displayed/enabled candidates yet.");
                        return null;
                    }

                    foreach (var c in candidates)
                    {
                        IWebElement clickable = c;

                        var tag = (clickable.TagName ?? string.Empty).ToLowerInvariant();
                        if (tag == "span" || tag == "div")
                        {
                            var parentButton = TryFindAncestorClickable(clickable);
                            if (parentButton != null)
                            {
                                Logger.LogTrace("FindPhotosAndVideosOptionButton: climbed from tag={Tag} to ancestor tag={AncestorTag}",
                                    c.TagName, parentButton.TagName);
                                clickable = parentButton;
                            }
                            else
                            {
                                Logger.LogTrace("FindPhotosAndVideosOptionButton: no clickable ancestor found for tag={Tag}", c.TagName);
                            }
                        }

                        if (clickable.Displayed && clickable.Enabled)
                        {
                            Logger.LogDebug(
                                "FindPhotosAndVideosOptionButton: selected candidate tag={Tag} aria-label={AriaLabel} title={Title}",
                                clickable.TagName,
                                clickable.GetAttribute("aria-label"),
                                clickable.GetAttribute("title")
                            );
                            return clickable;
                        }
                    }

                    return null;
                });

                Logger.LogDebug("FindPhotosAndVideosOptionButton completed. found={Found}", result is not null);
                return result;
            }
            catch (WebDriverTimeoutException ex)
            {
                Logger.LogError(ex,
                    "FindPhotosAndVideosOptionButton timed out after {Timeout}. xpath='{XPath}'",
                    timeout, FindPhotosAndVideosOption
                );
                throw;
            }
        }

        private IWebElement? TryFindAncestorClickable(IWebElement element)
        {
            try
            {
                return element.FindElement(By.XPath(
                    "ancestor::*[self::button or @role='button' or @role='menuitem'][1]"
                ));
            }
            catch (Exception ex)
            {
                Logger.LogTrace(ex, "TryFindAncestorClickable: no clickable ancestor found.");
                return null;
            }
        }
    }
}
