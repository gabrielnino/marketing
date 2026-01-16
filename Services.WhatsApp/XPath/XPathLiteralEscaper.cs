using Microsoft.Extensions.Logging;
using Services.WhatsApp.Abstractions.XPath;
using Services.WhatsApp.Login;

namespace Services.WhatsApp.XPath
{
    internal sealed class XPathLiteralEscaper(ILogger<LoginService> logger) : IXPathLiteralEscaper
    {
        public string EscapeXPathLiteral(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            logger.LogDebug(
                "EscapeXPathLiteral started. valueLength={ValueLength}",
                value.Length
            );

            if (!value.Contains("'"))
            {
                logger.LogDebug("EscapeXPathLiteral: Using single-quoted XPath literal.");
                return $"'{value}'";
            }

            if (!value.Contains("\""))
            {
                logger.LogDebug("EscapeXPathLiteral: Using double-quoted XPath literal.");
                return $"\"{value}\"";
            }

            logger.LogDebug("EscapeXPathLiteral: Using concat() XPath literal strategy.");

            var parts = value.Split('\'');

            var partsString = string.Join(", \"'\", ", parts.Select(p => $"'{p}'"));
            var result = "concat(" + partsString + ")";

            logger.LogDebug(
                "EscapeXPathLiteral completed. partCount={PartCount}",
                parts.Length
            );

            return result;
        }
    }
}
