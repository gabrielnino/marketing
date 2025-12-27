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

        // aria-label="Type a message"
        private IWebDriver Driver { get; } = driver;
        public ILogger<WhatsAppChatService> Logger { get; } = logger;
        private AppConfig Config { get; } = config;
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

            var escapedPath = imagePath.Replace("\"", "\"\"");

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

            try { File.Delete(scriptPath); } catch { /* ignore */ }

            if (proc.ExitCode != 0)
            {
                var err = proc.StandardError.ReadToEnd();
                throw new InvalidOperationException($"AutoIt script failed. ExitCode={proc.ExitCode}. {err}");
            }
        }

        public Task SendMessageAsync(
                ImageMessagePayload imageMessagePayload,
                TimeSpan? timeout = null,
                TimeSpan? pollInterval = null,
                CancellationToken ct = default)
        {
            Logger.LogInformation("SendMessageAsync started. messageLength={MessageLength}", imageMessagePayload.Caption?.Length ?? 0);

            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(imageMessagePayload.Caption))
            {
                Logger.LogWarning("SendMessageAsync aborted: message is null or whitespace.");
                throw new ArgumentException("Message cannot be empty.", nameof(imageMessagePayload.Caption));
            }

            Logger.LogInformation("Step 1/3: Locating WhatsApp compose box...");
           
            Logger.LogInformation("Locating attach button using XPath '{XPath}'...", XpathToFindAttachButton);
            TimeSpan loginTimeout = Config.WhatsApp.LoginTimeout;
            TimeSpan loginPollInterval = Config.WhatsApp.LoginPollInterval;
            var attachButton = FindAttachButton(loginTimeout, loginPollInterval);
            if (attachButton is null)
            {
                Logger.LogError("Attach button not found. XPath='{XPath}'", XpathToFindAttachButton);
                throw new NoSuchElementException("Attach button not found.");
            }

            Logger.LogInformation("Clicking attach button...");
            attachButton.Click();

            Logger.LogInformation("Locating 'Photos & videos' option using XPath '{XPath}'...", FindPhotosAndVideosOption);
            var photoAndVideo = FindPhotosAndVideosOptionButton(loginTimeout, loginPollInterval);
            if (photoAndVideo is null)
            {
                Logger.LogError("'Photos & videos' option not found. XPath='{XPath}'", FindPhotosAndVideosOption);
                throw new NoSuchElementException("'Photos & videos' option not found.");
            }

            Logger.LogInformation("Clicking 'Photos & videos' option...");
            photoAndVideo.Click();

            Logger.LogInformation("Opening file dialog via AutoIT...");
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

            Logger.LogInformation("Locating caption element using XPath '{XPath}'...", XpathFindCaption);
            var caption = FindCaption(loginTimeout, loginPollInterval);
            if (caption is null)
            {
                Logger.LogError("Caption element not found. XPath='{XPath}'", XpathFindCaption);
                throw new NoSuchElementException("Caption element not found.");
            }

            Logger.LogInformation("Typing caption...");
            caption.SendKeys(imageMessagePayload.Caption);
            Logger.LogInformation("Submitting caption (Enter)...");
            caption.SendKeys(Keys.Enter);

            ct.ThrowIfCancellationRequested();

            Logger.LogInformation("Step 2/3: Focusing compose box...");
            

            ct.ThrowIfCancellationRequested();

            Logger.LogInformation("Step 3/3: Sending message ({Length} chars) and submitting...", imageMessagePayload.Caption.Length);
            Logger.LogInformation("SendMessageAsync completed successfully.");
            return Task.CompletedTask;
        }



        private IWebElement FindAttachButton(TimeSpan timeout, TimeSpan pollingInterval)
        {
            var wait = new WebDriverWait(Driver, timeout)
            {
                PollingInterval = pollingInterval
            };

            Logger.LogDebug("FindAttachButton: Finding attach button. xpath='{XPath}'", XpathToFindAttachButton);

            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
                );

            var attachButton = wait.Until(driver =>
            {
                var element = driver
                    .FindElements(By.XPath(XpathToFindAttachButton))
                    .FirstOrDefault();

                if (element is null)
                    return null;

                return element.Displayed && element.Enabled ? element : null;
            });

            Logger.LogDebug("FindAttachButton: Found={Found}", attachButton is not null);
            return attachButton;
        }

        private IWebElement FindPhotosAndVideosOptionButton(TimeSpan timeout, TimeSpan pollingInterval)
        {
            var wait = new WebDriverWait(Driver, timeout)
            {
                PollingInterval = pollingInterval
            };
            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
                );
            Logger.LogDebug("FindPhotosAndVideosOptionButton: Finding option. xpath='{XPath}'", FindPhotosAndVideosOption);

            var photosAndVideosOption = wait.Until(driver =>
            {
                var element = driver
                    .FindElements(By.XPath(FindPhotosAndVideosOption))
                    .FirstOrDefault();

                if (element is null)
                    return null;

                return element.Displayed && element.Enabled ? element : null;
            });

            //var photosAndVideosOption = Driver.FindElements(By.XPath(FindPhotosAndVideosOption)).FirstOrDefault();
            Logger.LogDebug("FindPhotosAndVideosOptionButton: Found={Found}", photosAndVideosOption is not null);
            return photosAndVideosOption;
        }

        private IWebElement FindCaption(TimeSpan timeout, TimeSpan pollingInterval)
        {
            var wait = new WebDriverWait(Driver, timeout)
            {
                PollingInterval = pollingInterval
            };
            wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException)
                );
            Logger.LogDebug("FindCaption: Finding caption candidate(s). xpath='{XPath}'", XpathFindCaption);
            //var send = Driver.FindElements(By.XPath(XpathFindCaption)).FirstOrDefault();
            var send = wait.Until(driver =>
            {
                var element = driver
                    .FindElements(By.XPath(XpathFindCaption))
                    .FirstOrDefault();

                if (element is null)
                    return null;

                return element.Displayed && element.Enabled ? element : null;
            });
            Logger.LogDebug("FindCaption: Found={Found}", send is not null);
            return send;
        }
    }
}
