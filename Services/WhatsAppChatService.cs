using System.Diagnostics;
using System.Text;
using System.Threading;
using Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Interfaces;
using Services.Messages;

namespace Services
{
    public sealed class WhatsAppChatService(
        IWebDriver driver,
        ILogger<WhatsAppChatService> logger,
        AppConfig config
        ) : IWhatsAppChatService
    {

        private const string XpathToFindAttachButton = "//button[@aria-label='Attach' and @type='button']";
        private const string FindPhotosAndVideosOption = "//li[@role='button']//span[normalize-space()='Photos & videos']/ancestor::li";
        private const string XpathFindCaption = "//div[@role='textbox' and @contenteditable='true' and @aria-label='Type a message']";

        private IWebDriver Driver { get; } = driver;
        public ILogger<WhatsAppChatService> Logger { get; } = logger;
        private AppConfig Config { get; } = config;
        private void OpenFileDialogWithAutoIT(string imagePath)
        {
            Logger.LogInformation("OpenFileDialogWithAutoIT started.");

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                Logger.LogError("OpenFileDialogWithAutoIT aborted: imagePath is null or whitespace.");
                throw new ArgumentException("imagePath is null/empty.", nameof(imagePath));
            }

            imagePath = Path.GetFullPath(imagePath);
            Logger.LogDebug("Resolved imagePath to '{ImagePath}'", imagePath);

            if (!File.Exists(imagePath))
            {
                Logger.LogError("Image file not found at path '{ImagePath}'", imagePath);
                throw new FileNotFoundException("Image file not found.", imagePath);
            }

            var autoItExePath = @"C:\Program Files (x86)\AutoIt3\AutoIt3.exe";
            Logger.LogDebug("Using AutoIt executable at '{AutoItExePath}'", autoItExePath);

            if (!File.Exists(autoItExePath))
            {
                Logger.LogError("AutoIt executable not found at '{AutoItExePath}'", autoItExePath);
                throw new FileNotFoundException("AutoIt3.exe not found.", autoItExePath);
            }

            var escapedPath = imagePath.Replace("\"", "\"\"");
            Logger.LogDebug("Escaped image path for AutoIt.");

            Logger.LogDebug("Building AutoIt script...");
            var autoItScript = new StringBuilder()
                .AppendLine("; AutoIt Script - whatsapp_upload.au3")
                .AppendLine("Opt('WinTitleMatchMode', 2)")
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
                .AppendLine($"ControlSetText($title, '', '[CLASS:Edit; INSTANCE:1]', \"{escapedPath}\")")
                .AppendLine("Sleep(300)")
                .AppendLine("")
                .AppendLine("If ControlClick($title, '', '[CLASS:Button; INSTANCE:1]') = 0 Then")
                .AppendLine("    ControlSend($title, '', '[CLASS:Edit; INSTANCE:1]', '{ENTER}')")
                .AppendLine("EndIf")
                .AppendLine("")
                .AppendLine("Exit 0")
                .ToString();

            var scriptPath = Path.Combine(Path.GetTempPath(), $"whatsapp_upload_{Guid.NewGuid():N}.au3");
            Logger.LogDebug("Writing AutoIt script to '{ScriptPath}'", scriptPath);
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

            Logger.LogInformation("Starting AutoIt process...");
            using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start AutoIt process.");

            if (!proc.WaitForExit(15000))
            {
                Logger.LogWarning("AutoIt process timeout exceeded. Attempting to kill process.");
                try { proc.Kill(true); } catch { /* ignore */ }
                throw new TimeoutException("AutoIt file upload script timed out.");
            }

            Logger.LogInformation("AutoIt process exited with code {ExitCode}", proc.ExitCode);

            try
            {
                File.Delete(scriptPath);
                Logger.LogDebug("Temporary AutoIt script deleted.");
            }
            catch
            {
                Logger.LogWarning("Failed to delete temporary AutoIt script at '{ScriptPath}'", scriptPath);
            }

            if (proc.ExitCode != 0)
            {
                var err = proc.StandardError.ReadToEnd();
                Logger.LogError(
                    "AutoIt script failed. ExitCode={ExitCode}. Error={Error}",
                    proc.ExitCode,
                    err
                );
                throw new InvalidOperationException($"AutoIt script failed. ExitCode={proc.ExitCode}. {err}");
            }

            Logger.LogInformation("OpenFileDialogWithAutoIT completed successfully.");
        }


        public Task SendMessageAsync(
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
                OpenFileDialogWithAutoIT(imageMessagePayload.StoredImagePath);
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

            Logger.LogInformation("Typing caption...");
            caption.SendKeys(imageMessagePayload.Caption);

            Logger.LogInformation("Submitting caption (Enter)...");
            caption.SendKeys(Keys.Enter);

            ct.ThrowIfCancellationRequested();

            Logger.LogInformation("Step 2/3: Focusing compose box...");

            ct.ThrowIfCancellationRequested();

            Logger.LogInformation(
                "Step 3/3: Sending message ({Length} chars) and submitting...",
                imageMessagePayload.Caption.Length
            );

            Logger.LogInformation("SendMessageAsync completed successfully.");
            return Task.CompletedTask;
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
                timeout,
                pollingInterval,
                FindPhotosAndVideosOption
            );

            var wait = new WebDriverWait(Driver, timeout)
            {
                PollingInterval = pollingInterval
            };

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
            );

            IWebElement photosAndVideosOption;
            try
            {
                photosAndVideosOption = wait.Until(driver =>
                {
                    var element = driver
                        .FindElements(By.XPath(FindPhotosAndVideosOption))
                        .FirstOrDefault();

                    if (element is null)
                    {
                        Logger.LogTrace("FindPhotosAndVideosOptionButton: Option not present yet.");
                        return null;
                    }

                    if (!element.Displayed || !element.Enabled)
                    {
                        Logger.LogTrace(
                            "FindPhotosAndVideosOptionButton: Element found but not ready. displayed={Displayed}, enabled={Enabled}",
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
                    "FindPhotosAndVideosOptionButton timed out after {Timeout}. xpath='{XPath}'",
                    timeout,
                    FindPhotosAndVideosOption
                );
                throw;
            }

            Logger.LogDebug(
                "FindPhotosAndVideosOptionButton completed. found={Found} displayed={Displayed} enabled={Enabled}",
                photosAndVideosOption is not null,
                photosAndVideosOption?.Displayed,
                photosAndVideosOption?.Enabled
            );

            return photosAndVideosOption;
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
