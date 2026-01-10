using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.WhatsApp.Abstractions.Login;
using Services.WhatsApp.Abstractions.Selector;
using Services.WhatsApp.Abstractions.Selenium;

namespace Services.WhatsApp.Login
{
    internal sealed class WhatsAppLoginStateChecker(
           IWebDriverFacade driver,
           IWhatsAppSelectors selectors,
           ILogger<LoginService> logger) : IWhatsAppLoginStateChecker
    {
        public bool IsWhatsAppLoggedIn()
        {
            logger.LogDebug("IsWhatsAppLoggedIn: Checking WhatsApp Web login state...");

            try
            {
                var elements = driver.FindElements(By.CssSelector(selectors.CssSelectorToFindLoggedInMarker));
                var isLoggedIn = elements.Count > 0;

                logger.LogDebug(
                    "IsWhatsAppLoggedIn: Selector '{Selector}' returned {Count} elements. LoggedIn={IsLoggedIn}.",
                    selectors.CssSelectorToFindLoggedInMarker,
                    elements.Count,
                    isLoggedIn
                );

                return isLoggedIn;
            }
            catch (NoSuchElementException ex)
            {
                logger.LogWarning(
                    ex,
                    "IsWhatsAppLoggedIn: Selector '{Selector}' not found. Assuming not logged in.",
                    selectors.CssSelectorToFindLoggedInMarker
                );
                return false;
            }
            catch (WebDriverException ex)
            {
                logger.LogError(
                    ex,
                    "IsWhatsAppLoggedIn: WebDriver error while checking login state."
                );
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "IsWhatsAppLoggedIn: Unexpected error while checking login state."
                );
                return false;
            }
        }
    }
}
