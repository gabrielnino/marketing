using Application.Result;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Result
{
    public sealed class SerilogErrorLogger(ILogger<SerilogErrorLogger> logger) : IErrorLogger
    {
        public Task LogAsync(Exception ex, CancellationToken cancellationToken = default)
        {
            logger.LogError(ex, "Unhandled exception captured by IErrorLogger");
            return Task.CompletedTask;
        }
    }
}
