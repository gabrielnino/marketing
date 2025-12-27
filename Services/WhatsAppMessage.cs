using Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services
{
    public class WhatsAppMessage(
        ILogger<LoginService> logger,
        ILoginService loginService,
        ExecutionTracker executionOption,
        IWhatsAppChatService whatsAppChatService,
        AppConfig config
        ) : IWhatsAppMessage
    {
        public ILogger<LoginService> Logger { get; } = logger;
        public ILoginService Login { get; } = loginService;

        public ExecutionTracker ExecutionOption { get; } = executionOption;
        public IWhatsAppChatService WhatsAppChatService { get; } = whatsAppChatService;
        private AppConfig Config { get; } = config;

        public async Task SendMessageAsync()
        {
            Logger.LogInformation("WhatsAppMessage execution started");

            Logger.LogInformation("Starting WhatsApp login process");
            await Login.LoginAsync();
            Logger.LogInformation("WhatsApp login completed successfully");

            Logger.LogInformation("Finalizing execution folder");
            var finalizeReport = ExecutionOption.FinalizeByCopyThenDelete(true);
            LogFinalizeReport(finalizeReport);

            Logger.LogInformation(
                "Beginning message dispatch. Total contacts: {ContactCount}",
                Config.WhatsApp.AllowedChatTargets.Count);

            foreach (var contact in Config.WhatsApp.AllowedChatTargets)
            {
                Logger.LogInformation("Opening chat for contact: {Contact}", contact);

                await WhatsAppChatService.OpenContactChatAsync(
                    contact,
                    Config.WhatsApp.LoginPollInterval,
                    Config.WhatsApp.LoginTimeout);

                Logger.LogInformation("Chat opened successfully for contact: {Contact}", contact);

                Logger.LogInformation("Sending message to contact: {Contact}", contact);

                await WhatsAppChatService.SendMessageAsync(
                    "hola mundo",
                    Config.WhatsApp.LoginPollInterval,
                    Config.WhatsApp.LoginTimeout);

                Logger.LogInformation("Message sent successfully to contact: {Contact}", contact);
            }

            Logger.LogInformation("WhatsAppMessage execution finished");
        }

        public void LogFinalizeReport(FinalizeReport report)
        {
            if (report is null)
            {
                Logger.LogWarning("Finalize report is null");
                return;
            }

            Logger.LogInformation(
                "Finalizing execution from {RunningPath} to {FinishedPath}",
                report.RunningPath,
                report.FinishedPath);

            foreach (var failure in report.CopyFailures)
            {
                Logger.LogError(
                    failure.Exception,
                    "Copy failed during execution finalization: {Path}",
                    failure.Path);
            }

            foreach (var failure in report.DeleteFailures)
            {
                Logger.LogWarning(
                    failure.Exception,
                    "Delete failed for running execution folder: {Path}",
                    failure.Path);
            }

            if (report.IsClean)
            {
                Logger.LogInformation("Execution finalized successfully");
            }
            else
            {
                Logger.LogWarning(
                    "Execution finalized with issues. CopyFailures={CopyFailures}, DeleteFailures={DeleteFailures}",
                    report.CopyFailures.Count,
                    report.DeleteFailures.Count);
            }
        }
    }
}
