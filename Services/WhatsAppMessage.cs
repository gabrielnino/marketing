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
    public class WhatsAppMessage(
        AppConfig config,
        IWebDriverFactory driverFactory,
        ILogger<LoginService> logger,
        ICaptureSnapshot capture,
        ExecutionTracker executionOptions,
        ISecurityCheck securityCheck,
        IDirectoryCheck directoryCheck) : IWhatsAppMessage
    {
        public AppConfig Config { get; } = config;
        public IWebDriverFactory DriverFactory { get; } = driverFactory;
        public ILogger<LoginService> Logger { get; } = logger;
        public ICaptureSnapshot Capture { get; } = capture;
        public ExecutionTracker ExecutionOptions { get; } = executionOptions;
        public ISecurityCheck SecurityCheck { get; } = securityCheck;
        public IDirectoryCheck DirectoryCheck { get; } = directoryCheck;

        public Task SendMessageAsync()
        {
            throw new NotImplementedException();
        }
    }
}
