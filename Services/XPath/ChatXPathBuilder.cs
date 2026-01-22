using Services.Abstractions.XPath;

namespace Services.XPath
{
    internal sealed class ChatXPathBuilder(IXPathLiteralEscaper escaper) : IChatXPathBuilder
    {
        public string GetXpathToFind(string needleLowerInvariant)
        {
            return $"//span[contains(translate(@title,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), {escaper.EscapeXPathLiteral(needleLowerInvariant)})]";
        }
    }
}
