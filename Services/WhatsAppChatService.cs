using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Interfaces;

namespace Services
{
    public sealed class WhatsAppChatService(IWebDriver driver) : IWhatsAppChatService
    {
        private IWebDriver Driver { get; } = driver;

        private bool IsWhatsAppLoggedIn()
        {
            try
            {
                // Strong logged-in signal: message compose box
                return Driver.FindElements(
                    By.CssSelector("div[role='textbox'][contenteditable='true']")
                ).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task OpenContactChatAsync(string chatIdentifier, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            // 1️⃣ Ensure user is logged in BEFORE doing anything
            if (!IsWhatsAppLoggedIn())
            {
                throw new InvalidOperationException(
                    "WhatsApp Web is not logged in. Call LoginAsync() before opening a chat."
                );
            }
            await TypeIntoSearchBoxAsync(chatIdentifier, TimeSpan.FromSeconds(15));
            await ClickChatByTitleAsync(chatIdentifier, TimeSpan.FromSeconds(15));
            await Task.CompletedTask;
        }



        private async Task TypeIntoSearchBoxAsync(
            string text,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty.", nameof(text));

            timeout ??= TimeSpan.FromSeconds(10);
            pollInterval ??= TimeSpan.FromMilliseconds(200);

            var deadline = DateTimeOffset.UtcNow + timeout.Value;

            while (DateTimeOffset.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var input = Driver
                        .FindElements(By.CssSelector(
                            "div[role='textbox'][contenteditable='true'][aria-label='Search input textbox']"
                        ))
                        .FirstOrDefault();

                    if (input is { Displayed: true, Enabled: true })
                    {
                        input.Click();

                        // Clear existing content
                        input.SendKeys(Keys.Control + "a");
                        input.SendKeys(Keys.Backspace);

                        input.SendKeys(text);
                        input.SendKeys(Keys.Enter);

                        return;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    // DOM updated → retry
                }
                catch (InvalidElementStateException)
                {
                    // Not ready yet → retry
                }

                await Task.Delay(pollInterval.Value, ct);
            }

            throw new WebDriverTimeoutException(
                $"Search input textbox not available within {timeout.Value.TotalSeconds} seconds."
            );
        }


        private async Task ClickChatByTitleAsync(
            string chatTitle,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(chatTitle))
                throw new ArgumentException("Chat title cannot be empty.", nameof(chatTitle));

            timeout ??= TimeSpan.FromSeconds(10);
            pollInterval ??= TimeSpan.FromMilliseconds(250);

            var needle = chatTitle.Trim().ToLowerInvariant();
            var end = DateTimeOffset.UtcNow + timeout.Value;

            while (DateTimeOffset.UtcNow < end)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    // Match by title (case-insensitive)
                    var span = Driver.FindElements(By.XPath(
                        $"//span[contains(translate(@title,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), {EscapeXPathLiteral(needle)})]"
                    )).FirstOrDefault();

                    if (span is { Displayed: true })
                    {
                        // Click nearest likely-clickable ancestor
                        var target = span.FindElements(By.XPath("./ancestor::*[@role='gridcell' or @role='row' or @tabindex][1]"))
                                         .FirstOrDefault()
                                     ?? span;

                        if (target.Displayed && target.Enabled)
                        {
                            target.Click();
                            return;
                        }
                    }
                }
                catch (StaleElementReferenceException)
                {
                    // rerender → retry
                }
                catch (NoSuchElementException)
                {
                    // ancestor not found → retry
                }

                await Task.Delay(pollInterval.Value, ct);
            }

            throw new WebDriverTimeoutException($"Chat not found or not clickable: '{chatTitle}'.");
        }


        private static string EscapeXPathLiteral(string value)
        {
            if (!value.Contains("'"))
                return $"'{value}'";

            if (!value.Contains("\""))
                return $"\"{value}\"";

            // concat('a', "'", 'b')
            var parts = value.Split('\'');
            return "concat(" + string.Join(", \"'\", ", parts.Select(p => $"'{p}'")) + ")";
        }


        public Task SendMessageAsync(string message,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty.", nameof(message));

            var box = FindComposeBox();
            box.Click();
            box.SendKeys(message);
            box.SendKeys(Keys.Enter);

            return Task.CompletedTask;
        }


        private IWebElement FindComposeBox()
        {
            var boxes = Driver.FindElements(By.CssSelector("div[role='textbox'][contenteditable='true']"));
            if (boxes.Count == 0)
                throw new NoSuchElementException("Compose textbox not found.");
            return boxes[^1]; // heuristic
        }

    }
}
