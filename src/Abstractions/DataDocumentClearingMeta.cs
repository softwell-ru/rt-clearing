using SoftWell.Fpml.Confirmation;
using SoftWell.RtCodes;

namespace SoftWell.RtClearing;

/// <summary>
/// Метаинформация для DataDocument
/// </summary>
internal class DataDocumentClearingMeta : IClearingMeta
{
    private const string _clearingOrganizationRole = "ClearingOrganization";

    private readonly Lazy<Trade> _trade;

    private readonly Lazy<Party> _ourParty;

    private readonly Documentation? _documentation;

    public DataDocumentClearingMeta(DataDocument dataDocument)
    {
        DataDocument = dataDocument ?? throw new ArgumentNullException(nameof(dataDocument));
        _trade = new Lazy<Trade>(() => GetTradeOrDefaut(DataDocument) ?? throw new InvalidOperationException("Document contains no trades"));
        _ourParty = new Lazy<Party>(() => GetOurPartyOrDefault(DataDocument) ?? throw new InvalidOperationException("Document doesn't contain our party"));
        _documentation = GetDocumentation(DataDocument);
    }

    public DataDocument DataDocument { get; }

    public Document Document => DataDocument;

    /// <inheritdoc />
    public string GetClearingOrganizationPartyId()
    {
        var partyReference = GetClearingOrganizationPartyReferenceOrDefault(_trade.Value, _ourParty.Value)
            ?? throw new InvalidOperationException("Document trade contains no clearing party reference for our party");

        return GetPartyByHref(partyReference.href);
    }

    /// <inheritdoc />
    public string GetProductId()
    {
        var productId = _trade.Value.Item.productId?.FirstOrDefault(
                x => x?.Value is not null && x?.productIdScheme == RtSchemes.ProductId)
                ?.Value
                ?? throw new InvalidOperationException($"Trade does not contain ProductId");
        return productId;
    }

    public string? GetComment()
    {
        return _documentation?.attachment?.FirstOrDefault()?.resourceType?.Value;
    }

    /// <inheritdoc />
    public string GetPartyByHref(string partyHref)
    {
        ArgumentNullException.ThrowIfNull(partyHref);

        var party = GetPartyByHrefOrDefault(DataDocument, partyHref)
            ?? throw new InvalidOperationException($"Document contains no party with href '{partyHref}'");

        return GetPartyId(party);
    }

    /// <inheritdoc />
    public string GetTradeId()
    {
        if (_trade.Value?.tradeHeader?.partyTradeIdentifier is not null)
        {
            foreach (var pti in _trade.Value.tradeHeader.partyTradeIdentifier)
            {
                if (pti.Items is null) continue;

                foreach (var tradeId in pti.Items.OfType<TradeId>())
                {
                    if (tradeId?.Value is not null) return tradeId.Value;
                }
            }
        }

        throw new InvalidOperationException("Trade contains no trade id");
    }

    /// <inheritdoc />
    public string GetOnBehalfOf()
    {
        return GetPartyId(_ourParty.Value);
    }

    internal static Trade? GetTradeOrDefaut(DataDocument dataDocument)
    {
        // RuTerminal отправляет один ордер в документе
        return dataDocument.trade?.FirstOrDefault();
    }

    internal static Party? GetOurPartyOrDefault(DataDocument dataDocument)
    {
        var href = GetOurPartyHrefOrDefault(dataDocument);
        if (href is null) return null;

        return GetPartyByHrefOrDefault(dataDocument, href);
    }

    internal static Documentation? GetDocumentation(DataDocument dataDocument)
    {
        var documentation = dataDocument?.trade.FirstOrDefault()?.documentation;
        return documentation;
    }

    internal static string? GetOurPartyHrefOrDefault(DataDocument dataDocument)
    {
        var href = dataDocument.onBehalfOf?.partyReference?.href;
        return href;
    }

    internal static PartyReference? GetClearingOrganizationPartyReferenceOrDefault(Trade trade, Party forParty)
    {
        if (trade.tradeHeader?.partyTradeInformation is null) return null;

        foreach (var pti in trade.tradeHeader.partyTradeInformation)
        {
            if (pti?.partyReference?.href != forParty.id) continue;
            if (pti?.relatedParty is null) continue;

            foreach (var rp in pti.relatedParty)
            {
                if (rp?.role?.Value?.Equals(_clearingOrganizationRole, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return rp.partyReference;
                }
            }
        }

        return null;
    }

    private static Party? GetPartyByHrefOrDefault(DataDocument dataDocument, string href)
    {
        return dataDocument.party?.FirstOrDefault(x => x?.id == href);
    }

    private static string GetPartyId(Party party)
    {
        var partyId = party.Items?.OfType<PartyId>()?.FirstOrDefault()
            ?? throw new InvalidOperationException($"Party '{party.id}' contains no party id");

        return partyId.Value ?? throw new InvalidOperationException($"Party id of party '{party.id}' is empty");
    }
}