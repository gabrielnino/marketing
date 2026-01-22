namespace Services.Abstractions.Selector
{
    public interface ISelectors
    {
        string CssSelectorToFindLoggedInMarker { get; }
        string CssSelectorToFindSearchInput { get; }
        string XpathToFindGridcellAncestor { get; }
    }
}
