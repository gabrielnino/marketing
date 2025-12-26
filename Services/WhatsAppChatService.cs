using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            await Task.CompletedTask;
        }

        //private async Task WaitForChatOrErrorAsync(TimeSpan timeout, CancellationToken ct)
        //{
        //    var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, timeout);

        //    await Task.Run(() =>
        //    {
        //        wait.Until(d =>
        //        {
        //            ct.ThrowIfCancellationRequested();

        //            // Chat ready
        //            if (d.FindElements(By.CssSelector(
        //                "div[role='textbox'][contenteditable='true']")
        //            ).Count > 0)
        //                return true;

        //            // Error detection
        //            var body = d.FindElements(By.TagName("body"))
        //                        .FirstOrDefault()?.Text ?? string.Empty;

        //            if (body.Contains("isn't on WhatsApp", StringComparison.OrdinalIgnoreCase) ||
        //                body.Contains("invalid", StringComparison.OrdinalIgnoreCase))
        //            {
        //                throw new InvalidOperationException(
        //                    "WhatsApp reports the number is invalid or unavailable."
        //                );
        //            }

        //            return false;
        //        });
        //    }, ct);
        //}


        public Task SendMessageAsync(string message, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty.", nameof(message));

            var box = FindComposeBox();
            box.Click();
            box.SendKeys(message);
            box.SendKeys(Keys.Enter);

            return Task.CompletedTask;
        }

        private void WaitForChatOrError(TimeSpan timeout, CancellationToken ct)
        {
            var wait = new WebDriverWait(Driver, timeout);

            wait.Until(d =>
            {
                ct.ThrowIfCancellationRequested();

                // Compose textbox present => chat ready
                if (d.FindElements(By.CssSelector("div[role='textbox'][contenteditable='true']")).Count > 0)
                    return true;

                // Basic error detection
                var body = d.FindElements(By.TagName("body")).FirstOrDefault()?.Text ?? "";
                if (body.Contains("isn't on WhatsApp", StringComparison.OrdinalIgnoreCase) ||
                    body.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("WhatsApp indicates the phone number is invalid or not available.");

                return false;
            });
        }

        private IWebElement FindComposeBox()
        {
            var boxes = Driver.FindElements(By.CssSelector("div[role='textbox'][contenteditable='true']"));
            if (boxes.Count == 0)
                throw new NoSuchElementException("Compose textbox not found.");
            return boxes[^1]; // heuristic
        }

        private static string NormalizeE164ToDigits(string e164)
            => System.Text.RegularExpressions.Regex.Replace(e164, @"\D", "");
    }
}
