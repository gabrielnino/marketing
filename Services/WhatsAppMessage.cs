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
        ILoginService loginService) : IWhatsAppMessage
    {
        public ILogger<LoginService> Logger { get; } = logger;
        public ILoginService Login { get; } = loginService;
        


        public async Task SendMessageAsync()
        {
            await Login.LoginAsync();
        }
    }
}
