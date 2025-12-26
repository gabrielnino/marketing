using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services
{
    public class WhatsAppMessage(ILogger<LoginService> logger,
        ILoginService loginService, ExecutionTracker executionOption) : IWhatsAppMessage
    {
        public ILogger<LoginService> Logger { get; } = logger;
        public ILoginService Login { get; } = loginService;

        public ExecutionTracker ExecutionOption { get; } = executionOption;

        public async Task SendMessageAsync()
        {
            await Login.LoginAsync();
            var finalizeReport = ExecutionOption.FinalizeByCopyThenDelete(true);
            LogFinalizeReport(finalizeReport);
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
