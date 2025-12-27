using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;

namespace Services
{
    public sealed class WhatsAppChatService(
        IWebDriver driver,
        ILogger<LoginService> logger
        ) : IWhatsAppChatService
    {
        private const string WhatAppMessage = "WhatsApp Web is not logged in. Call LoginAsync() before opening a chat.";
        private const string CssSelectorToFind = "div[role='textbox'][contenteditable='true']";
        private const string CssSelectorToFindSearchInput = "div[role='textbox'][contenteditable='true'][aria-label='Search input textbox']";
        private const string XpathToFindGridcell = "./ancestor::*[@role='gridcell' or @role='row' or @tabindex][1]";
        private const string CssSelectorToFindTextbox = "div[role='textbox'][contenteditable='true']";
        private const string XpathToFindAttachButton = "//button[@aria-label='Attach' and @type='button']";
        private const string FindPhotosAndVideosOption = "//li[@role='button']//span[normalize-space()='Photos & videos']/ancestor::li";
        private const string XpathFindCaption = "//div[@contenteditable='true'] | " + "//div[@role='textbox'] | " + "//textarea";

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
        private static void OpenFileDialogWithAutoIT(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException("imagePath is null/empty.", nameof(imagePath));

            imagePath = Path.GetFullPath(imagePath);
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("Image file not found.", imagePath);

            var autoItExePath = @"C:\Program Files (x86)\AutoIt3\AutoIt3.exe";
            if (!File.Exists(autoItExePath))
                throw new FileNotFoundException("AutoIt3.exe not found.", autoItExePath);

            // Escape for AutoIt string literal
            // AutoIt uses " for strings, so we double it inside the path.
            var escapedPath = imagePath.Replace("\"", "\"\"");

            // Support both English and Spanish dialog titles (Open / Abrir)
            // Use WinWaitActive with OR by checking one then the other.
            var autoItScript = new StringBuilder()
                .AppendLine("; AutoIt Script - whatsapp_upload.au3")
                .AppendLine("Opt('WinTitleMatchMode', 2)") // partial match for safer title handling
                .AppendLine("Local $timeout = 10")
                .AppendLine("")
                .AppendLine("If WinWaitActive('Open', '', $timeout) = 0 Then")
                .AppendLine("    If WinWaitActive('Abrir', '', $timeout) = 0 Then")
                .AppendLine("        Exit 1")
                .AppendLine("    EndIf")
                .AppendLine("EndIf")
                .AppendLine("")
                .AppendLine("Local $title = ''")
                .AppendLine("If WinActive('Open') Then")
                .AppendLine("    $title = 'Open'")
                .AppendLine("ElseIf WinActive('Abrir') Then")
                .AppendLine("    $title = 'Abrir'")
                .AppendLine("Else")
                .AppendLine("    Exit 2")
                .AppendLine("EndIf")
                .AppendLine("")
                .AppendLine("; Set file path into File name edit box")
                .AppendLine($"ControlSetText($title, '', '[CLASS:Edit; INSTANCE:1]', \"{escapedPath}\")")
                .AppendLine("Sleep(300)")
                .AppendLine("")
                .AppendLine("; Prefer clicking the Open button; fallback to ENTER if click fails")
                .AppendLine("If ControlClick($title, '', '[CLASS:Button; INSTANCE:1]') = 0 Then")
                .AppendLine("    ControlSend($title, '', '[CLASS:Edit; INSTANCE:1]', '{ENTER}')")
                .AppendLine("EndIf")
                .AppendLine("")
                .AppendLine("Exit 0")
                .ToString();

            var scriptPath = Path.Combine(Path.GetTempPath(), $"whatsapp_upload_{Guid.NewGuid():N}.au3");
            File.WriteAllText(scriptPath, autoItScript, Encoding.UTF8);

            var psi = new ProcessStartInfo
            {
                FileName = autoItExePath,
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start AutoIt process.");
            if (!proc.WaitForExit(15000))
            {
                try { proc.Kill(true); } catch { /* ignore */ }
                throw new TimeoutException("AutoIt file upload script timed out.");
            }

            // Cleanup temp script (best effort)
            try { File.Delete(scriptPath); } catch { /* ignore */ }

            if (proc.ExitCode != 0)
            {
                var err = proc.StandardError.ReadToEnd();
                throw new InvalidOperationException($"AutoIt script failed. ExitCode={proc.ExitCode}. {err}");
            }
        }



        public Task SendMessageAsync(
                string message,
                TimeSpan? timeout = null,
                TimeSpan? pollInterval = null,
                CancellationToken ct = default)
        {
            Logger.LogInformation("SendMessageAsync started.");

            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(message))
            {
                Logger.LogWarning("SendMessageAsync aborted: message is null or whitespace.");
                throw new ArgumentException("Message cannot be empty.", nameof(message));
            }

            Logger.LogInformation("Step 1/3: Locating WhatsApp compose box...");
            IWebElement box;
            try
            {
                box = FindComposeBox();
                Logger.LogInformation("Step 1/3: Compose box found.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SendMessageAsync failed: Unable to locate compose box.");
                throw;
            }


            var attachButton = FindAttachButton();
            attachButton.Click();

            var photoAndVideo = FindPhotosAndVideosOptionButton();
            photoAndVideo.Click();

            OpenFileDialogWithAutoIT("E:\\Company\\whatappmessage\\superO.png");
            var caption = FindCaption();
            caption.SendKeys("This is an automated message with image.");
            caption.SendKeys(Keys.Enter);
            ct.ThrowIfCancellationRequested();

            Logger.LogInformation("Step 2/3: Focusing compose box...");
            try
            {
                box.Click();
                Logger.LogInformation("Step 2/3: Compose box focused.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SendMessageAsync failed: Unable to focus compose box.");
                throw;
            }

            ct.ThrowIfCancellationRequested();

            Logger.LogInformation("Step 3/3: Sending message ({Length} chars) and submitting...", message.Length);
            try
            {
                box.SendKeys(message);
                box.SendKeys(Keys.Enter);
                Logger.LogInformation("Step 3/3: Message sent successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SendMessageAsync failed while sending message.");
                throw;
            }

            return Task.CompletedTask;
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



        private IWebElement FindAttachButton()
        {
            var attachButton = Driver.FindElements(By.XPath(XpathToFindAttachButton)).FirstOrDefault();
            return attachButton;
        }

        private IWebElement FindPhotosAndVideosOptionButton()
        {
            var photosAndVideosOption = Driver.FindElements(By.XPath(FindPhotosAndVideosOption)).FirstOrDefault();
            return photosAndVideosOption;
        }



        private IWebElement FindCaption()
        {
            var send = Driver.FindElements(By.XPath(XpathFindCaption)).FirstOrDefault();
            return send;
        }

        private string GetXpathToFind(string needle)
        {
            return $"//span[contains(translate(@title,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), {EscapeXPathLiteral(needle)})]";
        }


        private IWebElement FindComposeBox()
        {
            Logger.LogDebug(
                "FindComposeBox: Locating compose textbox using selector '{Selector}'...",
                CssSelectorToFindTextbox
            );

            try
            {
                var boxes = Driver.FindElements(By.CssSelector(CssSelectorToFindTextbox));

                Logger.LogDebug(
                    "FindComposeBox: Selector '{Selector}' returned {Count} elements.",
                    CssSelectorToFindTextbox,
                    boxes.Count
                );

                if (boxes.Count == 0)
                {
                    Logger.LogError(
                        "FindComposeBox: Compose textbox not found (0 matches). Selector='{Selector}'.",
                        CssSelectorToFindTextbox
                    );
                    throw new NoSuchElementException("Compose textbox not found.");
                }

                var selected = boxes[^1]; // heuristic

                Logger.LogDebug(
                    "FindComposeBox: Selected last textbox (heuristic). displayed={Displayed} enabled={Enabled}",
                    selected.Displayed,
                    selected.Enabled
                );

                return selected;
            }
            catch (WebDriverException ex)
            {
                Logger.LogError(ex, "FindComposeBox: WebDriver error while locating compose textbox.");
                throw;
            }
        }

        private string EscapeXPathLiteral(string value)
        {

            if (value is null)
            {
                // Defensive: upstream callers should not pass null, but fail fast if they do
                throw new ArgumentNullException(nameof(value));
            }

            Logger.LogDebug(
                "EscapeXPathLiteral started. valueLength={ValueLength}",
                value.Length
            );

            // Case 1: contains no single quotes → wrap in single quotes
            if (!value.Contains("'"))
            {
                Logger.LogDebug("EscapeXPathLiteral: Using single-quoted XPath literal.");
                return $"'{value}'";
            }

            // Case 2: contains single quotes but no double quotes → wrap in double quotes
            if (!value.Contains("\""))
            {
                Logger.LogDebug("EscapeXPathLiteral: Using double-quoted XPath literal.");
                return $"\"{value}\"";
            }

            // Case 3: contains both → use concat()
            Logger.LogDebug("EscapeXPathLiteral: Using concat() XPath literal strategy.");

            // concat('a', "'", 'b')
            var parts = value.Split('\'');

            var partsString = string.Join(", \"'\", ", parts.Select(p => $"'{p}'"));
            var result = "concat(" + partsString + ")";

            Logger.LogDebug(
                "EscapeXPathLiteral completed. partCount={PartCount}",
                parts.Length
            );

            return result;
        }

    }
}
