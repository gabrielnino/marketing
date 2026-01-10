namespace Services.WhatsApp.Abstractions.Selector
{
    public interface IWhatsAppSelectors
    {
        string CssSelectorToFindLoggedInMarker { get; }
        string CssSelectorToFindSearchInput { get; }
        string XpathToFindGridcellAncestor { get; }
    }
}
