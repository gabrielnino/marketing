namespace Services.WhatsApp.Abstractions.XPath
{
    public interface IChatXPathBuilder
    {
        string GetXpathToFind(string needleLowerInvariant);
    }
}
