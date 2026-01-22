using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Abstractions.Login;
using Services.Abstractions.Selector;
using Services.Abstractions.Selenium;

namespace Services.Login
{
    internal sealed class LoginStateChecker(
           IWebDriverFacade driver,
           ISelectors selectors,
           ILogger<LoginService> logger) : ILoginStateChecker
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
