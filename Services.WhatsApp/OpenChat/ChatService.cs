using Configuration;
using Domain.WhatsApp;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Interfaces;
using Services.WhatsApp.Abstractions.OpenChat;
using Services.WhatsApp.Abstractions.Search;

namespace Services.WhatsApp.OpenChat
{
    public sealed class ChatService(
        IWebDriver driver,
        ILogger<ChatService> logger,
        AppConfig config,
        IAutoItRunner autoItRunner,
        IAttachments controls
    ) : IChatService
    {
  


        private const string XpathFindCaption =
            "//div[@role='textbox' and @contenteditable='true' and @aria-label='Type a message']";

        private IWebDriver Driver { get; } = driver;
        public ILogger<ChatService> Logger { get; } = logger;
        private AppConfig Config { get; } = config;
        private IAutoItRunner AutoItRunner { get; } = autoItRunner;
        private IAttachments Controls { get; } = controls;

        public async Task SendMessageAsync(
            ImageMessagePayload imageMessagePayload,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            Logger.LogInformation(
                "SendMessageAsync started. hasPayload={HasPayload} messageLength={MessageLength} storedImagePath='{StoredImagePath}'",
                imageMessagePayload is not null,
                imageMessagePayload?.Caption?.Length ?? 0,
                imageMessagePayload?.StoredImagePath
            );

            ct.ThrowIfCancellationRequested();

            if (imageMessagePayload is null)
            {
                Logger.LogError("SendMessageAsync aborted: imageMessagePayload is null.");
                throw new ArgumentNullException(nameof(imageMessagePayload));
            }

            if (string.IsNullOrWhiteSpace(imageMessagePayload.Caption))
            {
                Logger.LogWarning("SendMessageAsync aborted: Caption is null/empty/whitespace.");
                throw new ArgumentException("Message cannot be empty.", nameof(imageMessagePayload.Caption));
            }

            if (string.IsNullOrWhiteSpace(imageMessagePayload.StoredImagePath))
            {
                Logger.LogError("SendMessageAsync aborted: StoredImagePath is null/empty.");
                throw new ArgumentException("StoredImagePath cannot be empty.", nameof(imageMessagePayload.StoredImagePath));
            }

            if (!File.Exists(imageMessagePayload.StoredImagePath))
            {
                Logger.LogError("SendMessageAsync aborted: StoredImagePath does not exist. path='{Path}'", imageMessagePayload.StoredImagePath);
                throw new FileNotFoundException("Image file not found.", imageMessagePayload.StoredImagePath);
            }

            TimeSpan loginTimeout = timeout ?? Config.WhatsApp.LoginTimeout;
            TimeSpan loginPollInterval = pollInterval ?? Config.WhatsApp.LoginPollInterval;

            Logger.LogInformation(
                "Using timeouts. timeout={Timeout} pollInterval={PollInterval}",
                loginTimeout, loginPollInterval
            );

            Logger.LogInformation("Step 1/3: Locating attach button...");
            var attachButton = Controls.FindAttachButton(loginTimeout, loginPollInterval);
            if (attachButton is null)
            {
                throw new NoSuchElementException("Attach button not found.");
            }

            Logger.LogDebug(
                "Attach button found. tag={Tag} displayed={Displayed}, enabled={Enabled} aria-label={AriaLabel} title={Title}",
                attachButton.TagName,
                attachButton.Displayed,
                attachButton.Enabled,
                attachButton.GetAttribute("aria-label"),
                attachButton.GetAttribute("title")
            );

            Logger.LogInformation("Clicking attach button...");
            attachButton.Click();
            Logger.LogDebug("Attach button clicked.");


            var photoAndVideo = controls.FindPhotosAndVideosOptionButton(loginTimeout, loginPollInterval);
            if (photoAndVideo is null)
            {
                throw new NoSuchElementException("'Photos & videos' option not found.");
            }

            Logger.LogDebug(
                "'Photos & videos' option found. tag={Tag} displayed={Displayed}, enabled={Enabled} aria-label={AriaLabel} title={Title}",
                photoAndVideo.TagName,
                photoAndVideo.Displayed,
                photoAndVideo.Enabled,
                photoAndVideo.GetAttribute("aria-label"),
                photoAndVideo.GetAttribute("title")
            );

            Logger.LogInformation("Clicking 'Photos & videos' option...");
            photoAndVideo.Click();
            Logger.LogDebug("'Photos & videos' option clicked.");

            Logger.LogInformation(
                "Opening file dialog via AutoIT... storedImagePath='{StoredImagePath}' useInterpreter={UseInterpreter}",
                imageMessagePayload.StoredImagePath,
                true
            );

            AutoItRunnerResult autoItRunnerResult;
            try
            {
                autoItRunnerResult = await AutoItRunner.RunAsync(
                    timeout: loginTimeout,
                    imagePath: imageMessagePayload.StoredImagePath,
                    useAutoItInterpreter: true,
                    cancellationToken: ct
                );

                Logger.LogInformation(
                    "AutoIT finished. exitCode={ExitCode} timedOut={TimedOut} duration={Duration} logFilePath='{LogFilePath}'",
                    autoItRunnerResult.ExitCode,
                    autoItRunnerResult.TimedOut,
                    autoItRunnerResult.Duration,
                    autoItRunnerResult.LogFilePath
                );

                if (autoItRunnerResult.TimedOut)
                {
                    Logger.LogError("AutoIT timed out. duration={Duration} logFilePath='{LogFilePath}'",
                        autoItRunnerResult.Duration, autoItRunnerResult.LogFilePath);
                    throw new TimeoutException("AutoIT timed out while selecting file.");
                }

                if (autoItRunnerResult.ExitCode != 0)
                {
                    Logger.LogError(
                        "AutoIT failed. exitCode={ExitCode} stderrLength={StdErrLength} stdoutLength={StdOutLength} logFilePath='{LogFilePath}'",
                        autoItRunnerResult.ExitCode,
                        autoItRunnerResult.StdErr?.Length ?? 0,
                        autoItRunnerResult.StdOut?.Length ?? 0,
                        autoItRunnerResult.LogFilePath
                    );
                    throw new InvalidOperationException($"AutoIT failed with exit code {autoItRunnerResult.ExitCode}.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "AutoIT failed while selecting file. storedImagePath='{StoredImagePath}'", imageMessagePayload.StoredImagePath);
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
                "Caption element found. tag={Tag} displayed={Displayed}, enabled={Enabled} aria-label={AriaLabel}",
                caption.TagName,
                caption.Displayed,
                caption.Enabled,
                caption.GetAttribute("aria-label")
            );

            Logger.LogInformation("Typing caption via execCommand (emoji-safe)... captionLength={Length}", imageMessagePayload.Caption.Length);

            try
            {
                SetCaptionViaExecCommand(caption, imageMessagePayload.Caption ?? string.Empty);
                Logger.LogDebug("Caption text injected via execCommand.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed while setting caption via execCommand.");
                throw;
            }

            Logger.LogInformation("Submitting caption (Enter)...");
            caption.SendKeys(Keys.Enter);
            Logger.LogDebug("Enter sent to caption element.");

            ct.ThrowIfCancellationRequested();

            Logger.LogInformation("Step 2/3: Focusing compose box...");
            // (No-op in current implementation; log kept as stage marker)

            ct.ThrowIfCancellationRequested();

            Logger.LogInformation(
                "Step 3/3: Sending message completed. length={Length}",
                imageMessagePayload.Caption.Length
            );

            Logger.LogInformation("SendMessageAsync completed successfully.");
        }

        private void SetCaptionViaExecCommand(IWebElement element, string text)
        {
            Logger.LogDebug("SetCaptionViaExecCommand started. textLength={Length}", text?.Length ?? 0);

            if (Driver is not IJavaScriptExecutor js)
            {
                Logger.LogError("Driver does not support JavaScript execution. driverType={DriverType}", Driver.GetType().FullName);
                throw new NotSupportedException("Driver does not support JavaScript execution.");
            }

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

            Logger.LogDebug("SetCaptionViaExecCommand completed.");
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
