using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Logging
{
    public static class ApiPayloadLogger
    {
        private static readonly JsonSerializerOptions PrettyJson = new()
        {
            WriteIndented = true
        };

        public static void LogRequest(
            ILogger logger,
            string runId,
            string operation,
            object payload)
        {
            var json = JsonSerializer.Serialize(payload, PrettyJson);

            logger.LogInformation(
                "[RUN {RunId}] [{Operation}] REQUEST PAYLOAD:\n{Json}",
                runId,
                operation,
                json);
        }

        public static void LogResponse(
            ILogger logger,
            string runId,
            string operation,
            string rawJson)
        {
            logger.LogInformation(
                "[RUN {RunId}] [{Operation}] RESPONSE PAYLOAD:\n{Json}",
                runId,
                operation,
                rawJson);
        }
    }
}
