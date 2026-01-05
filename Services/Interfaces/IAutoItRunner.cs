using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.WhatsApp;

namespace Services.Interfaces
{
    public interface IAutoItRunner
    {
        public Task<AutoItRunnerResult> RunAsync(
            TimeSpan timeout,
            string imagePath,
            bool useAutoItInterpreter = false,
            CancellationToken cancellationToken = default);
    }
}
