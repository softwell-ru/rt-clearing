using SoftWell.Fpml.Confirmation;

namespace SoftWell.RtClearing;

/// <inheritdoc />
/// <remarks>Пока что работает только с DataDocument</remarks>
public class DefaultClearingMetaExtractor : IClearingMetaExtractor
{
    /// <inheritdoc />
    public bool IsClearingRequested(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document is not DataDocument dataDocument) return false;
        var trade = DataDocumentClearingMeta.GetTradeOrDefaut(dataDocument);

        if (trade is null) return false;

        var ourParty = DataDocumentClearingMeta.GetOurPartyOrDefault(dataDocument);

        if (ourParty is null) return false;

        var clearingPartyRef = DataDocumentClearingMeta.GetClearingOrganizationPartyReferenceOrDefault(trade, ourParty);

        return clearingPartyRef is not null;
    }

    /// <inheritdoc />
    public IClearingMeta Extract(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document is not DataDocument dataDocument) throw new InvalidOperationException("Document is not a data document");

        return new DataDocumentClearingMeta(dataDocument);
    }
}