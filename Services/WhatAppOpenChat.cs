using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;
using Keys = OpenQA.Selenium.Keys;

namespace Services
{
    public class WhatAppOpenChat(
        IWebDriver driver,
        ILogger<LoginService> logger
        ) : IWhatAppOpenChat
    {
        private const string WhatAppMessage = "WhatsApp Web is not logged in. Call LoginAsync() before opening a chat.";
        private const string CssSelectorToFind = "div[role='textbox'][contenteditable='true']";
        private const string CssSelectorToFindSearchInput = "div[role='textbox'][contenteditable='true'][aria-label='Search input textbox']";
        private const string XpathToFindGridcell = "./ancestor::*[@role='gridcell' or @role='row' or @tabindex][1]";


        // aria-label="Type a message"
        private IWebDriver Driver { get; } = driver;
        public ILogger<LoginService> Logger { get; } = logger;

        public async Task OpenContactChatAsync(
            string chatIdentifier,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            Logger.LogInformation("OpenContactChatAsync started. chatIdentifier='{ChatIdentifier}'", chatIdentifier);

            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(chatIdentifier))
            {
                Logger.LogWarning("OpenContactChatAsync aborted: chatIdentifier is null/empty/whitespace.");
                throw new ArgumentException("Chat identifier cannot be null, empty, or whitespace.", nameof(chatIdentifier));
            }

            // 1) Logged-in check
            Logger.LogInformation("Step 1/4: Checking WhatsApp Web login state...");
            if (!IsWhatsAppLoggedIn())
            {
                Logger.LogError("OpenContactChatAsync failed: WhatsApp Web is not logged in.");
                throw new InvalidOperationException(WhatAppMessage);
            }
            Logger.LogInformation("Step 1/4: Logged in confirmed.");

            // 2) Resolve timeouts
            var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(10);
            var effectivePoll = pollInterval ?? TimeSpan.FromMilliseconds(200);

            if (effectiveTimeout <= TimeSpan.Zero)
            {
                Logger.LogWarning("Invalid timeout provided: {Timeout}.", effectiveTimeout);
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
            }

            if (effectivePoll <= TimeSpan.Zero)
            {
                Logger.LogWarning("Invalid pollInterval provided: {PollInterval}.", effectivePoll);
                throw new ArgumentOutOfRangeException(nameof(pollInterval), "Poll interval must be greater than zero.");
            }

            if (effectivePoll > effectiveTimeout)
            {
                var adjusted = TimeSpan.FromMilliseconds(Math.Max(50, effectiveTimeout.TotalMilliseconds / 10));
                Logger.LogWarning(
                    "PollInterval {PollInterval} > Timeout {Timeout}. Adjusting pollInterval to {AdjustedPollInterval}.",
                    effectivePoll, effectiveTimeout, adjusted);

                effectivePoll = adjusted;
            }

            Logger.LogInformation(
                "Step 2/4: Using timeout={Timeout} pollInterval={PollInterval}.",
                effectiveTimeout, effectivePoll);

            ct.ThrowIfCancellationRequested();

            // 3) Type into search box
            Logger.LogInformation("Step 3/4: Typing chatIdentifier into WhatsApp search box...");
            try
            {
                await TypeIntoSearchBoxAsync(chatIdentifier, effectiveTimeout, effectivePoll /*, ct */)
                    .ConfigureAwait(false);

                Logger.LogInformation("Step 3/4: Search input completed.");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("OpenContactChatAsync canceled during Step 3/4 (TypeIntoSearchBoxAsync).");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "OpenContactChatAsync failed during Step 3/4 (TypeIntoSearchBoxAsync).");
                throw;
            }

            ct.ThrowIfCancellationRequested();

            // 4) Click chat by title
            Logger.LogInformation("Step 4/4: Clicking chat by title '{ChatIdentifier}'...", chatIdentifier);
            try
            {
                await ClickChatByTitleAsync(chatIdentifier, effectiveTimeout, effectivePoll /*, ct */)
                    .ConfigureAwait(false);

                Logger.LogInformation("Step 4/4: Chat opened successfully. chatIdentifier='{ChatIdentifier}'", chatIdentifier);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("OpenContactChatAsync canceled during Step 4/4 (ClickChatByTitleAsync).");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "OpenContactChatAsync failed during Step 4/4 (ClickChatByTitleAsync).");
                throw;
            }
        }

        private bool IsWhatsAppLoggedIn()
        {
            Logger.LogDebug("IsWhatsAppLoggedIn: Checking WhatsApp Web login state...");

            try
            {
                var elements = Driver.FindElements(By.CssSelector(CssSelectorToFind));
                var isLoggedIn = elements.Count > 0;

                Logger.LogDebug(
                    "IsWhatsAppLoggedIn: Selector '{Selector}' returned {Count} elements. LoggedIn={IsLoggedIn}.",
                    CssSelectorToFind,
                    elements.Count,
                    isLoggedIn
                );

                return isLoggedIn;
            }
            catch (NoSuchElementException ex)
            {
                Logger.LogWarning(
                    ex,
                    "IsWhatsAppLoggedIn: Selector '{Selector}' not found. Assuming not logged in.",
                    CssSelectorToFind
                );
                return false;
            }
            catch (WebDriverException ex)
            {
                Logger.LogError(
                    ex,
                    "IsWhatsAppLoggedIn: WebDriver error while checking login state."
                );
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "IsWhatsAppLoggedIn: Unexpected error while checking login state."
                );
                return false;
            }
        }

        private async Task TypeIntoSearchBoxAsync(
            string text,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            Logger.LogInformation(
                "TypeIntoSearchBoxAsync started. textLength={TextLength}",
                text?.Length ?? 0
            );

            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.LogWarning("TypeIntoSearchBoxAsync aborted: text is null or whitespace.");
                throw new ArgumentException("Text cannot be empty.", nameof(text));
            }

            timeout ??= TimeSpan.FromSeconds(10);
            pollInterval ??= TimeSpan.FromMilliseconds(200);

            Logger.LogInformation(
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
                    Logger.LogDebug(
                        "Attempt {Attempt}: Locating search input using selector '{Selector}'.",
                        attempt,
                        CssSelectorToFindSearchInput
                    );

                    var input = Driver
                        .FindElements(By.CssSelector(CssSelectorToFindSearchInput))
                        .FirstOrDefault();

                    if (input is { Displayed: true, Enabled: true })
                    {
                        Logger.LogInformation(
                            "Search input found and ready on attempt {Attempt}. Focusing and typing...",
                            attempt
                        );

                        input.Click();

                        Logger.LogDebug("Clearing existing search input content.");
                        input.SendKeys(Keys.Control + "a");
                        input.SendKeys(Keys.Backspace);

                        Logger.LogDebug("Typing search text and submitting.");
                        input.SendKeys(text);
                        input.SendKeys(Keys.Enter);

                        Logger.LogInformation("TypeIntoSearchBoxAsync completed successfully.");
                        return;
                    }

                    Logger.LogDebug(
                        "Attempt {Attempt}: Search input not ready (null, hidden, or disabled).",
                        attempt
                    );
                }
                catch (StaleElementReferenceException)
                {
                    Logger.LogDebug(
                        "Attempt {Attempt}: StaleElementReferenceException encountered. Retrying...",
                        attempt
                    );
                }
                catch (InvalidElementStateException)
                {
                    Logger.LogDebug(
                        "Attempt {Attempt}: InvalidElementStateException encountered. Retrying...",
                        attempt
                    );
                }
                catch (Exception ex)
                {
                    Logger.LogError(
                        ex,
                        "TypeIntoSearchBoxAsync failed unexpectedly on attempt {Attempt}.",
                        attempt
                    );
                    throw;
                }

                await Task.Delay(pollInterval.Value, ct).ConfigureAwait(false);
            }

            Logger.LogError(
                "TypeIntoSearchBoxAsync timed out after {TimeoutSeconds} seconds.",
                timeout.Value.TotalSeconds
            );

            throw new WebDriverTimeoutException(
                $"Search input textbox not available within {timeout.Value.TotalSeconds} seconds."
            );
        }
        private string EscapeXPathLiteral(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Logger.LogDebug(
                "EscapeXPathLiteral started. valueLength={ValueLength}",
                value.Length
            );

            if (!value.Contains("'"))
            {
                Logger.LogDebug("EscapeXPathLiteral: Using single-quoted XPath literal.");
                return $"'{value}'";
            }

            if (!value.Contains("\""))
            {
                Logger.LogDebug("EscapeXPathLiteral: Using double-quoted XPath literal.");
                return $"\"{value}\"";
            }

            Logger.LogDebug("EscapeXPathLiteral: Using concat() XPath literal strategy.");

            var parts = value.Split('\'');

            var partsString = string.Join(", \"'\", ", parts.Select(p => $"'{p}'"));
            var result = "concat(" + partsString + ")";

            Logger.LogDebug(
                "EscapeXPathLiteral completed. partCount={PartCount}",
                parts.Length
            );

            return result;
        }
        private string GetXpathToFind(string needle)
        {
            return $"//span[contains(translate(@title,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), {EscapeXPathLiteral(needle)})]";
        }
        private async Task ClickChatByTitleAsync(
            string chatTitle,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            Logger.LogInformation(
                "ClickChatByTitleAsync started. chatTitleLength={ChatTitleLength}",
                chatTitle?.Length ?? 0
            );

            if (string.IsNullOrWhiteSpace(chatTitle))
            {
                Logger.LogWarning("ClickChatByTitleAsync aborted: chatTitle is null or whitespace.");
                throw new ArgumentException("Chat title cannot be empty.", nameof(chatTitle));
            }

            timeout ??= TimeSpan.FromSeconds(10);
            pollInterval ??= TimeSpan.FromMilliseconds(250);

            Logger.LogInformation(
                "Using timeout={Timeout} pollInterval={PollInterval}.",
                timeout, pollInterval
            );

            var needle = chatTitle.Trim().ToLowerInvariant();
            var end = DateTimeOffset.UtcNow + timeout.Value;

            Logger.LogDebug(
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
                    var xpathToFind = GetXpathToFind(needle);

                    Logger.LogDebug(
                        "Attempt {Attempt}: Searching chat span using XPath: {XPath}",
                        attempt,
                        xpathToFind
                    );

                    var span = Driver.FindElements(By.XPath(xpathToFind)).FirstOrDefault();

                    if (span is { Displayed: true })
                    {
                        Logger.LogInformation(
                            "Attempt {Attempt}: Matching chat span found and displayed. Resolving clickable target...",
                            attempt
                        );

                        var target = span.FindElements(By.XPath(XpathToFindGridcell)).FirstOrDefault() ?? span;

                        Logger.LogDebug(
                            "Attempt {Attempt}: Target resolved. targetDisplayed={Displayed} targetEnabled={Enabled}",
                            attempt,
                            target.Displayed,
                            target.Enabled
                        );

                        if (target.Displayed && target.Enabled)
                        {
                            Logger.LogInformation("Attempt {Attempt}: Clicking chat target...", attempt);
                            target.Click();
                            Logger.LogInformation("ClickChatByTitleAsync completed successfully.");
                            return;
                        }

                        Logger.LogDebug(
                            "Attempt {Attempt}: Target found but not clickable (displayed/enabled check failed).",
                            attempt
                        );
                    }
                    else
                    {
                        Logger.LogDebug(
                            "Attempt {Attempt}: No displayed span matched the chat title yet.",
                            attempt
                        );
                    }
                }
                catch (StaleElementReferenceException)
                {
                    Logger.LogDebug(
                        "Attempt {Attempt}: StaleElementReferenceException encountered (DOM rerender). Retrying...",
                        attempt
                    );
                }
                catch (NoSuchElementException)
                {
                    Logger.LogDebug(
                        "Attempt {Attempt}: NoSuchElementException encountered (ancestor/target missing). Retrying...",
                        attempt
                    );
                }
                catch (Exception ex)
                {
                    Logger.LogError(
                        ex,
                        "ClickChatByTitleAsync failed unexpectedly on attempt {Attempt}.",
                        attempt
                    );
                    throw;
                }

                await Task.Delay(pollInterval.Value, ct).ConfigureAwait(false);
            }

            Logger.LogError(
                "ClickChatByTitleAsync timed out after {TimeoutSeconds} seconds. Chat not found/clickable. chatTitleLength={ChatTitleLength}",
                timeout.Value.TotalSeconds,
                chatTitle.Length
            );

            throw new WebDriverTimeoutException($"Chat not found or not clickable: '{chatTitle}'.");
        }
    }
    
}
