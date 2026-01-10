using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.WhatsApp.Abstractions.Search;
using Services.WhatsApp.Abstractions.Selector;
using Services.WhatsApp.Abstractions.Selenium;
using Services.WhatsApp.Login;

namespace Services.WhatsApp.Search
{
    internal sealed class WhatsAppSearchBoxTyper(
           IWebDriverFacade driver,
           IWhatsAppSelectors selectors,
           ILogger<LoginService> logger) : IWhatsAppSearchBoxTyper
    {
        public async Task TypeIntoSearchBoxAsync(
            string text,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            logger.LogInformation(
                "TypeIntoSearchBoxAsync started. textLength={TextLength}",
                text?.Length ?? 0
            );

            if (string.IsNullOrWhiteSpace(text))
            {
                logger.LogWarning("TypeIntoSearchBoxAsync aborted: text is null or whitespace.");
                throw new ArgumentException("Text cannot be empty.", nameof(text));
            }

            timeout ??= TimeSpan.FromSeconds(10);
            pollInterval ??= TimeSpan.FromMilliseconds(200);

            logger.LogInformation(
                "Using timeout={Timeout} pollInterval={PollInterval}.",
                timeout, pollInterval
            );

            var deadline = DateTimeOffset.UtcNow + timeout.Value;
            var attempt = 0;

            while (DateTimeOffset.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();
                attempt++;

                try
                {
                    logger.LogDebug(
                        "Attempt {Attempt}: Locating search input using selector '{Selector}'.",
                        attempt,
                        selectors.CssSelectorToFindSearchInput
                    );

                    var input = driver
                        .FindElements(By.CssSelector(selectors.CssSelectorToFindSearchInput))
                        .FirstOrDefault();

                    if (input is { Displayed: true, Enabled: true })
                    {
                        logger.LogInformation(
                            "Search input found and ready on attempt {Attempt}. Focusing and typing...",
                            attempt
                        );

                        input.Click();

                        logger.LogDebug("Clearing existing search input content.");
                        input.SendKeys(Keys.Control + "a");
                        input.SendKeys(Keys.Backspace);

                        logger.LogDebug("Typing search text and submitting.");
                        input.SendKeys(text);
                        input.SendKeys(Keys.Enter);

                        logger.LogInformation("TypeIntoSearchBoxAsync completed successfully.");
                        return;
                    }

                    logger.LogDebug(
                        "Attempt {Attempt}: Search input not ready (null, hidden, or disabled).",
                        attempt
                    );
                }
                catch (StaleElementReferenceException)
                {
                    logger.LogDebug(
                        "Attempt {Attempt}: StaleElementReferenceException encountered. Retrying...",
                        attempt
                    );
                }
                catch (InvalidElementStateException)
                {
                    logger.LogDebug(
                        "Attempt {Attempt}: InvalidElementStateException encountered. Retrying...",
                        attempt
                    );
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "TypeIntoSearchBoxAsync failed unexpectedly on attempt {Attempt}.",
                        attempt
                    );
                    throw;
                }

                await Task.Delay(pollInterval.Value, ct).ConfigureAwait(false);
            }

            logger.LogError(
                "TypeIntoSearchBoxAsync timed out after {TimeoutSeconds} seconds.",
                timeout.Value.TotalSeconds
            );

            throw new WebDriverTimeoutException(
                $"Search input textbox not available within {timeout.Value.TotalSeconds} seconds."
            );
        }
    }
}
