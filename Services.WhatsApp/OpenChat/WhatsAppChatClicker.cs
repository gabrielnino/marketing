using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.WhatsApp.Abstractions.Selector;
using Services.WhatsApp.Abstractions.Selenium;
using Services.WhatsApp.Abstractions.XPath;
using Services.WhatsApp.Login;

namespace Services.WhatsApp.OpenChat
{
    internal sealed class WhatsAppChatClicker(
            IWebDriverFacade driver,
            IWhatsAppSelectors selectors,
            IChatXPathBuilder xpathBuilder,
            ILogger<LoginService> logger) : IWhatsAppChatClicker
    {
        public async Task ClickChatByTitleAsync(
            string chatTitle,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            logger.LogInformation(
                "ClickChatByTitleAsync started. chatTitleLength={ChatTitleLength}",
                chatTitle?.Length ?? 0
            );

            if (string.IsNullOrWhiteSpace(chatTitle))
            {
                logger.LogWarning("ClickChatByTitleAsync aborted: chatTitle is null or whitespace.");
                throw new ArgumentException("Chat title cannot be empty.", nameof(chatTitle));
            }

            timeout ??= TimeSpan.FromSeconds(10);
            pollInterval ??= TimeSpan.FromMilliseconds(250);

            logger.LogInformation(
                "Using timeout={Timeout} pollInterval={PollInterval}.",
                timeout, pollInterval
            );

            var needle = chatTitle.Trim().ToLowerInvariant();
            var end = DateTimeOffset.UtcNow + timeout.Value;

            logger.LogDebug(
                "Normalized chat title for search. originalLength={OriginalLength} normalizedLength={NormalizedLength}",
                chatTitle.Length,
                needle.Length
            );

            var attempt = 0;

            while (DateTimeOffset.UtcNow < end)
            {
                ct.ThrowIfCancellationRequested();
                attempt++;

                try
                {
                    var xpathToFind = xpathBuilder.GetXpathToFind(needle);

                    logger.LogDebug(
                        "Attempt {Attempt}: Searching chat span using XPath: {XPath}",
                        attempt,
                        xpathToFind
                    );

                    var span = driver.FindElements(By.XPath(xpathToFind)).FirstOrDefault();

                    if (span is { Displayed: true })
                    {
                        logger.LogInformation(
                            "Attempt {Attempt}: Matching chat span found and displayed. Resolving clickable target...",
                            attempt
                        );

                        var target = span.FindElements(By.XPath(selectors.XpathToFindGridcellAncestor)).FirstOrDefault() ?? span;

                        logger.LogDebug(
                            "Attempt {Attempt}: Target resolved. targetDisplayed={Displayed} targetEnabled={Enabled}",
                            attempt,
                            target.Displayed,
                            target.Enabled
                        );

                        if (target.Displayed && target.Enabled)
                        {
                            logger.LogInformation("Attempt {Attempt}: Clicking chat target...", attempt);
                            target.Click();
                            logger.LogInformation("ClickChatByTitleAsync completed successfully.");
                            return;
                        }

                        logger.LogDebug(
                            "Attempt {Attempt}: Target found but not clickable (displayed/enabled check failed).",
                            attempt
                        );
                    }
                    else
                    {
                        logger.LogDebug(
                            "Attempt {Attempt}: No displayed span matched the chat title yet.",
                            attempt
                        );
                    }
                }
                catch (StaleElementReferenceException)
                {
                    logger.LogDebug(
                        "Attempt {Attempt}: StaleElementReferenceException encountered (DOM rerender). Retrying...",
                        attempt
                    );
                }
                catch (NoSuchElementException)
                {
                    logger.LogDebug(
                        "Attempt {Attempt}: NoSuchElementException encountered (ancestor/target missing). Retrying...",
                        attempt
                    );
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "ClickChatByTitleAsync failed unexpectedly on attempt {Attempt}.",
                        attempt
                    );
                    throw;
                }

                await Task.Delay(pollInterval.Value, ct).ConfigureAwait(false);
            }

            logger.LogError(
                "ClickChatByTitleAsync timed out after {TimeoutSeconds} seconds. Chat not found/clickable. chatTitleLength={ChatTitleLength}",
                timeout.Value.TotalSeconds,
                chatTitle.Length
            );

            throw new WebDriverTimeoutException($"Chat not found or not clickable: '{chatTitle}'.");
        }
    }
}
