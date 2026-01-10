using Services.WhatsApp.Abstractions.Selector;

namespace Services.WhatsApp.Selector
{
    internal sealed class WhatsAppSelectors : IWhatsAppSelectors
    {
        // Original constants preserved verbatim
        public string CssSelectorToFindLoggedInMarker => "div[role='textbox'][contenteditable='true']";
        public string CssSelectorToFindSearchInput => "div[role='textbox'][contenteditable='true'][aria-label='Search input textbox']";
        public string XpathToFindGridcellAncestor => "./ancestor::*[@role='gridcell' or @role='row' or @tabindex][1]";
    }
}
