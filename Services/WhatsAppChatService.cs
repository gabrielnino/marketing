using System.Diagnostics;
using System.Text;
using System.Threading;
using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Interfaces;
using Services.Messages;
using Keys = OpenQA.Selenium.Keys;

namespace Services
{
    public sealed class WhatsAppChatService(
        IWebDriver driver,
        ILogger<WhatsAppChatService> logger,
        AppConfig config,
        IAutoItRunner autoItRunner
        ) : IWhatsAppChatService
    {

        private const string XpathToFindAttachButton = "//button[@aria-label='Attach' or @title='Attach' or @data-testid='attach-button' or .//span[@data-icon='clip' or @data-icon='plus']]";
        private const string FindPhotosAndVideosOption = "//*[@role='button' or @role='menuitem' or self::button]" + "[@aria-label='Photos & videos' or title='Photos & videos' or @data-testid='attach-photos' or .//span[normalize-space(.)='Photos & videos']]";
        private const string XpathFindCaption = "//div[@role='textbox' and @contenteditable='true' and @aria-label='Type a message']";

        private IWebDriver Driver { get; } = driver;
        public ILogger<WhatsAppChatService> Logger { get; } = logger;
        private AppConfig Config { get; } = config;
        private IAutoItRunner AutoItRunner { get; } = autoItRunner;
        


        public async Task SendMessageAsync(
        ImageMessagePayload imageMessagePayload,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null,
        CancellationToken ct = default)
        {
            Logger.LogInformation(
                "SendMessageAsync started. messageLength={MessageLength}",
                imageMessagePayload?.Caption?.Length ?? 0
            );

            ct.ThrowIfCancellationRequested();

            if (imageMessagePayload is null)
            {
                Logger.LogError("SendMessageAsync aborted: imageMessagePayload is null.");
                throw new ArgumentNullException(nameof(imageMessagePayload));
            }

            if (string.IsNullOrWhiteSpace(imageMessagePayload.Caption))
            {
                Logger.LogWarning("SendMessageAsync aborted: message is null or whitespace.");
                throw new ArgumentException("Message cannot be empty.", nameof(imageMessagePayload.Caption));
            }

            Logger.LogInformation("Step 1/3: Locating WhatsApp compose box...");

            Logger.LogInformation(
                "Locating attach button using XPath '{XPath}'...",
                XpathToFindAttachButton
            );

            TimeSpan loginTimeout = Config.WhatsApp.LoginTimeout;
            TimeSpan loginPollInterval = Config.WhatsApp.LoginPollInterval;

            var attachButton = FindAttachButton(loginTimeout, loginPollInterval);
            if (attachButton is null)
            {
                Logger.LogError("Attach button not found. XPath='{XPath}'", XpathToFindAttachButton);
                throw new NoSuchElementException("Attach button not found.");
            }

            Logger.LogDebug(
                "Attach button found. displayed={Displayed}, enabled={Enabled}",
                attachButton.Displayed,
                attachButton.Enabled
            );

            Logger.LogInformation("Clicking attach button...");
            attachButton.Click();

            Logger.LogInformation(
                "Locating 'Photos & videos' option using XPath '{XPath}'...",
                FindPhotosAndVideosOption
            );

            var photoAndVideo = FindPhotosAndVideosOptionButton(loginTimeout, loginPollInterval);
            if (photoAndVideo is null)
            {
                Logger.LogError("'Photos & videos' option not found. XPath='{XPath}'", FindPhotosAndVideosOption);
                throw new NoSuchElementException("'Photos & videos' option not found.");
            }

            Logger.LogDebug(
                "'Photos & videos' option found. displayed={Displayed}, enabled={Enabled}",
                photoAndVideo.Displayed,
                photoAndVideo.Enabled
            );

            Logger.LogInformation("Clicking 'Photos & videos' option...");
            photoAndVideo.Click();

            Logger.LogInformation("Opening file dialog via AutoIT... storedImagePath='{StoredImagePath}'", imageMessagePayload.StoredImagePath);
            try
            {
                var autoItRunnerResult = await AutoItRunner.RunAsync(
                    timeout: Config.WhatsApp.LoginTimeout,
                    imagePath: imageMessagePayload.StoredImagePath,
                    useAutoItInterpreter: true,
                    cancellationToken: ct
                );
     
                Logger.LogInformation("AutoIT completed file selection.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "AutoIT failed while selecting file.");
                throw;
            }

            Logger.LogInformation(
                "Locating caption element using XPath '{XPath}'...",
                XpathFindCaption
            );

            var caption = FindCaption(loginTimeout, loginPollInterval);
            if (caption is null)
            {
                Logger.LogError("Caption element not found. XPath='{XPath}'", XpathFindCaption);
                throw new NoSuchElementException("Caption element not found.");
            }

            Logger.LogDebug(
                "Caption element found. displayed={Displayed}, enabled={Enabled}",
                caption.Displayed,
                caption.Enabled
            );


            Logger.LogInformation("Typing caption via execCommand (emoji-safe)...");
            SetCaptionViaExecCommand(caption, imageMessagePayload.Caption ?? string.Empty);

            Logger.LogInformation("Submitting caption...");
            caption.SendKeys(OpenQA.Selenium.Keys.Enter);

            ct.ThrowIfCancellationRequested();

            Logger.LogInformation("Step 2/3: Focusing compose box...");

            ct.ThrowIfCancellationRequested();

            Logger.LogInformation(
                "Step 3/3: Sending message ({Length} chars) and submitting...",
                imageMessagePayload.Caption.Length
            );

            Logger.LogInformation("SendMessageAsync completed successfully.");
        }
        private void SetCaptionViaExecCommand(IWebElement element, string text)
        {
            if (Driver is not IJavaScriptExecutor js)
                throw new NotSupportedException("Driver does not support JavaScript execution.");

            js.ExecuteScript(@"
        const el = arguments[0];
        const value = arguments[1] ?? '';

        el.focus();

        // clear existing content
        document.execCommand('selectAll', false, null);
        document.execCommand('delete', false, null);

        // insert text (this fires the right events in many React contenteditables)
        document.execCommand('insertText', false, value);
    ", element, text);
        }
        private IWebElement FindAttachButton(TimeSpan timeout, TimeSpan pollingInterval)
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


        private IWebElement FindPhotosAndVideosOptionButton(TimeSpan timeout, TimeSpan pollingInterval)
        {
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
                return wait.Until(driver =>
                {
                    // Get ALL candidates and filter to the ones that are actually clickable.
                    var candidates = driver.FindElements(By.XPath(FindPhotosAndVideosOption))
                                           .Where(e => e.Displayed && e.Enabled)
                                           .ToList();

                    if (candidates.Count == 0)
                    {
                        Logger.LogTrace("FindPhotosAndVideosOptionButton: no displayed/enabled candidates yet.");
                        return null;
                    }

                    // If XPath matched a <span>, climb to the nearest clickable container.
                    foreach (var c in candidates)
                    {
                        IWebElement clickable = c;

                        // If it's a span or non-interactive node, climb up to a button/role=button/menuitem.
                        var tag = (clickable.TagName ?? string.Empty).ToLowerInvariant();
                        if (tag == "span" || tag == "div")
                        {
                            var parentButton = TryFindAncestorClickable(clickable);
                            if (parentButton != null)
                                clickable = parentButton;
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
                // nearest ancestor that is a real clickable option
                return element.FindElement(By.XPath(
                    "ancestor::*[self::button or @role='button' or @role='menuitem'][1]"
                ));
            }
            catch
            {
                return null;
            }
        }



        private IWebElement FindCaption(TimeSpan timeout, TimeSpan pollingInterval)
        {
            Logger.LogDebug(
                "FindCaption started. timeout={Timeout} pollingInterval={PollingInterval} xpath='{XPath}'",
                timeout,
                pollingInterval,
                XpathFindCaption
            );

            var wait = new WebDriverWait(Driver, timeout)
            {
                PollingInterval = pollingInterval
            };

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            IWebElement caption;
            try
            {
                caption = wait.Until(driver =>
                {
                    var element = driver
                        .FindElements(By.XPath(XpathFindCaption))
                        .FirstOrDefault();

                    if (element is null)
                    {
                        Logger.LogTrace("FindCaption: Caption element not present yet.");
                        return null;
                    }

                    if (!element.Displayed || !element.Enabled)
                    {
                        Logger.LogTrace(
                            "FindCaption: Element found but not ready. displayed={Displayed}, enabled={Enabled}",
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
                    "FindCaption timed out after {Timeout}. xpath='{XPath}'",
                    timeout,
                    XpathFindCaption
                );
                throw;
            }

            Logger.LogDebug(
                "FindCaption completed. found={Found} displayed={Displayed} enabled={Enabled}",
                caption is not null,
                caption?.Displayed,
                caption?.Enabled
            );

            return caption;
        }

    }
}
