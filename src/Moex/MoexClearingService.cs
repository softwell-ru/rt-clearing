using Microsoft.Extensions.Logging;
using QuickFix.Fields;
using QuickFix.FIX50SP2;
using SoftWell.Fix.Initiator;
using SoftWell.Fix.Initiator.MessagesHandling;
using SoftWell.Fpml.Confirmation;
using SoftWell.RtClearing.Moex.Configuration;
using SoftWell.RtCodes;

namespace SoftWell.RtClearing.Moex;

public class MoexClearingService : IClearingService, IFixMessagesHandler<ExecutionReport>
{
    /// <summary>
    /// Уникальный код используемый биржей в качестве контрагента  при мэтчинге заявок через matchref
    /// </summary> 
    private const string _matchRefCode = "*Всем";

    private readonly MoexClearingOptions _options;

    private readonly IFixMessagesSender _moexFixMessagesSender;

    private readonly ICodesConverter _codesConverter;

    private readonly IRtTradeReporter _rtTradeReporter;

    private readonly ILogger<MoexClearingService> _logger;

    public MoexClearingService(
        MoexClearingOptions options,
        IFixMessagesSender moexFixMessagesSender,
        ICodesConverter codesConverter,
        IRtTradeReporter rtTradeReporter,
        ILogger<MoexClearingService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _moexFixMessagesSender = moexFixMessagesSender ?? throw new ArgumentNullException(nameof(moexFixMessagesSender));
        _codesConverter = codesConverter ?? throw new ArgumentNullException(nameof(codesConverter));
        _rtTradeReporter = rtTradeReporter ?? throw new ArgumentNullException(nameof(rtTradeReporter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RequestClearingAsync(IClearingMeta meta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(meta);

        var order = await ConvertToNewOrderSingleAsync(meta, ct);
        await _moexFixMessagesSender.SendMessageAsync(order, ct);
    }

    public Task HandleMessageAsync(ExecutionReport message, CancellationToken ct = default)
    {
        if (!message.IsSetClOrdID())
        {
            _logger.LogWarning("В ExecutionReport нет обязательного поля ClOrdID. Message: {ExecutionReport}", message.ToString());
            return Task.CompletedTask;
        }

        if (!message.IsSetOrderID())
        {
            _logger.LogWarning("В ExecutionReport нет обязательного поля OrderID. Message: {ExecutionReport}", message.ToString());
            return Task.CompletedTask;
        }

        var id = message.ClOrdID.getValue();
        var externalId = message.OrderID.getValue();

        using var _ = _logger.BeginScope("{TradeId}", id);

        if (!message.IsSetOrdStatus())
        {
            _logger.LogWarning("В ExecutionReport нет обязательного поля OrdStatus. Message: {ExecutionReport}", message.ToString());
            return Task.CompletedTask;
        }

        var ordStatus = message.OrdStatus.getValue();

        var text = GetTextIfExists(message);

        switch (ordStatus)
        {
            case OrdStatus.NEW:
                _logger.LogDebug("Ордер {TradeId} был принят на рассмотрение: {Text}", id, text);
                break;

            case OrdStatus.PARTIALLY_FILLED:
                _logger.LogDebug("Ордер {TradeId} частично исполнен: {Text}", id, text);
                break;

            case OrdStatus.FILLED:
                _logger.LogDebug("Ордер {TradeId} исполнен: {Text}", id, text);
                break;

            case OrdStatus.REJECTED:
                _logger.LogWarning("Ордер {TradeId} был отклонен: {TextReason} (reason: {Reason})", id, text, message.IsSetOrdRejReason() ? message.OrdRejReason.getValue() : null);
                break;

            default:
                _logger.LogWarning("Получили отчет {ExecutionReport} для ордера {TradeId} с неизвестным статусом '{OrdStatus}'", message.ToString(), id, message.OrdStatus.getValue());
                break;
        }

        return _rtTradeReporter.ReportStatusChangedAsync(
            new TradeMatchingStatusChanged
            {
                TimestampUtc = message.IsSetTransactTime() ? message.TransactTime.getValue() : DateTime.UtcNow,
                TradeId = id,
                ExternalTradeId = externalId,
                ExternalSystem = "MOEX",
                TradingSessionId = "CPCL",
                Status = MapStatus(ordStatus),
                Comment = text
            },
            ct);
    }

    private async ValueTask<NewOrderSingle> ConvertToNewOrderSingleAsync(IClearingMeta meta, CancellationToken ct)
    {
        if (meta.Document is not DataDocument dataDocument) throw new InvalidOperationException("Document is not DataDocument");
        var trade = dataDocument.trade?.FirstOrDefault() ?? throw new InvalidOperationException("Document does not contain any trade");
        var onBehalfOfHref = dataDocument.onBehalfOf?.partyReference?.href ?? throw new InvalidOperationException("Document does not contain OnBehalfOf party reference");

        if (trade.Item is FxSingleLeg fxSingleLeg)
        {
            return await ConvertToNewOrderSingleAsync(meta, fxSingleLeg, onBehalfOfHref, ct);
        }
        else if (trade.Item is FxSwap fxSwap)
        {
            return await ConvertToNewOrderSingleAsync(meta, fxSwap, onBehalfOfHref, ct);
        }

        throw new NotImplementedException($"Product type {trade.Item.GetType()} is not supported");
    }

    private async ValueTask<NewOrderSingle> ConvertToNewOrderSingleAsync(IClearingMeta meta, FxSwap fxSwap, string onBehalfOfHref, CancellationToken ct)
    {
        var res = await GetNewOrderSingleTemplateAsync(meta, onBehalfOfHref, ct);

        var weBuy = GetTradeSideFxSwap(fxSwap, onBehalfOfHref);
        res.Side = new Side(weBuy ? Side.BUY : Side.SELL);

        var leadCcy = GetCcyFxSwap(fxSwap);
        var amount = fxSwap.nearLeg.exchangedCurrency1.paymentAmount.currency.Value == leadCcy ? fxSwap.nearLeg.exchangedCurrency1.paymentAmount.amount : fxSwap.nearLeg.exchangedCurrency2.paymentAmount.amount;
        res.OrderQty = new OrderQty(amount);

        var nearLegPrice = fxSwap.nearLeg.exchangeRate?.rate ?? throw new InvalidOperationException($"NearLeg does not contain price");
        var farLegPrice = fxSwap.farLeg.exchangeRate?.rate ?? throw new InvalidOperationException($"FarLeg does not contain price");
        res.Price = new QuickFix.Fields.Price(farLegPrice - nearLegPrice);
        res.Price2 = new QuickFix.Fields.Price2(farLegPrice);

        var leadReceiverPartyHref = fxSwap.nearLeg.exchangedCurrency1.payerPartyReference.href ?? throw new InvalidOperationException("Lead payment does not contain receiver party href");
        var leadPayerPartyHref = fxSwap.nearLeg.exchangedCurrency1.receiverPartyReference.href ?? throw new InvalidOperationException("Lead payment does not contain payer party href");

        var contraParty = meta.GetPartyByHref(weBuy ? leadReceiverPartyHref : leadPayerPartyHref);

        switch(_options.UseMatchRefSource)
        {
            case MatchRefDirection.Comment:
                await AddPartyAsync(res, _matchRefCode, QuickFix.Fields.PartyRole.CONTRA_FIRM, ct);
                res.ClOrdLinkID = new ClOrdLinkID(GetMatchRef(meta));
                break;
            default:
                await AddPartyAsync(res, contraParty, QuickFix.Fields.PartyRole.CONTRA_FIRM, ct);
                break;
        }

        res.OrdType = new OrdType(OrdType.FOREX_SWAP);

        return res;
    }

    private async ValueTask<NewOrderSingle> ConvertToNewOrderSingleAsync(IClearingMeta meta, FxSingleLeg fxSingleLeg, string onBehalfOfHref, CancellationToken ct)
    {
        var res = await GetNewOrderSingleTemplateAsync(meta, onBehalfOfHref, ct);

        var (leadPayment, secondPayment) = GetPayments(fxSingleLeg);

        var leadReceiverPartyHref = leadPayment.receiverPartyReference?.href ?? throw new InvalidOperationException("Lead payment does not contain receiver party href");
        var leadPayerPartyHref = leadPayment.payerPartyReference?.href ?? throw new InvalidOperationException("Lead payment does not contain payer party href");

        var weBuy = onBehalfOfHref == leadReceiverPartyHref;
        res.Side = new Side(weBuy ? Side.BUY : Side.SELL);

        var amount = leadPayment.paymentAmount?.amount ?? throw new InvalidOperationException($"Lead payment does not contain amount");
        res.OrderQty = new OrderQty(amount);

        var price = secondPayment.paymentAmount?.amount ?? throw new InvalidOperationException($"Second payment does not contain amount");
        res.Price = new QuickFix.Fields.Price(price / amount);

        var contraParty = meta.GetPartyByHref(weBuy ? leadReceiverPartyHref : leadPayerPartyHref);

        switch(_options.UseMatchRefSource)
        {
            case MatchRefDirection.Comment:
                await AddPartyAsync(res, _matchRefCode, QuickFix.Fields.PartyRole.CONTRA_FIRM, ct);
                res.ClOrdLinkID = new ClOrdLinkID(GetMatchRef(meta));
                break;
            default:
                await AddPartyAsync(res, contraParty, QuickFix.Fields.PartyRole.CONTRA_FIRM, ct);
                break;
        }

        res.OrdType = new OrdType(OrdType.LIMIT);

        return res;
    }

    private async ValueTask<NewOrderSingle> GetNewOrderSingleTemplateAsync(IClearingMeta meta, string onBehalfOfHref, CancellationToken ct)
    {
        var res = new NewOrderSingle
        {
            TransactTime = new TransactTime(DateTime.UtcNow),
            Product = new QuickFix.Fields.Product(QuickFix.Fields.Product.CURRENCY),
            TimeInForce = new TimeInForce(TimeInForce.GOOD_TILL_CANCEL),
            ClOrdID = new ClOrdID(meta.GetTradeId()),
            Account = new QuickFix.Fields.Account(_options.AccountId)
        };

        res.AddGroup(new NewOrderSingle.NoTradingSessionsGroup
        {
            TradingSessionID = new TradingSessionID("CPCL")
        });

        var symbol = await _codesConverter.ConvertRtProductIdToMoexAsync(meta.GetProductId(), ct);
        res.Symbol = new Symbol(symbol);

        var ourParty = meta.GetPartyByHref(onBehalfOfHref);
        await AddPartyAsync(res, ourParty, QuickFix.Fields.PartyRole.EXECUTING_FIRM, ct);

        return res;
    }

    private async ValueTask AddPartyAsync(NewOrderSingle order, string rtParty, int partyRole, CancellationToken ct)
    {
        var moexPartyId = await _codesConverter.ConvertRtPartyIdToMoexAsync(rtParty, ct);

        order.AddGroup(new NewOrderSingle.NoPartyIDsGroup
        {
            PartyIDSource = new PartyIDSource(PartyIDSource.PROPRIETARY),
            PartyID = new PartyID(moexPartyId),
            PartyRole = new QuickFix.Fields.PartyRole(partyRole)
        });
    }

    private static string  GetMatchRef(IClearingMeta meta) => meta.GetComment() ?? throw new InvalidOperationException("Comment not found");

    private static (Payment leadPayment, Payment secondPayment) GetPayments(FxSingleLeg fxSingleLeg)
    {
        var leadCurrencyHref = GetLeadCurrencyHref(fxSingleLeg);

        var exchangedCurrency1Href = fxSingleLeg.exchangedCurrency1?.paymentAmount?.currency?.Value
            ?? throw new InvalidOperationException("FxSingleLeg does not contain exchangedCurrency1 currency");
        var exchangedCurrency2Href = fxSingleLeg.exchangedCurrency2?.paymentAmount?.currency?.Value
            ?? throw new InvalidOperationException("FxSingleLeg does not contain exchangedCurrency2 currency");

        if (leadCurrencyHref == exchangedCurrency1Href)
        {
            return (fxSingleLeg.exchangedCurrency1, fxSingleLeg.exchangedCurrency2);
        }
        else if (leadCurrencyHref == exchangedCurrency2Href)
        {
            return (fxSingleLeg.exchangedCurrency2, fxSingleLeg.exchangedCurrency1);
        }

        throw new InvalidOperationException($"Lead currency is {leadCurrencyHref}, but FxSingleLeg contains only exchangedCurrency1='{exchangedCurrency1Href}' and exchangedCurrency2='{exchangedCurrency2Href}'");
    }

    private static bool GetTradeSideFxSwap(FxSwap fxSwap, string partyHref)
    {
        return fxSwap.nearLeg.exchangedCurrency1.receiverPartyReference.href.ToString() == partyHref;
    }

    private static string GetLeadCurrencyHref(FxSingleLeg fxSingleLeg)
    {
        var quoteBasis = fxSingleLeg.exchangeRate?.quotedCurrencyPair?.quoteBasis
            ?? throw new InvalidOperationException("FxSingleLeg does not contain QuoteBasis");

        return quoteBasis switch
        {
            QuoteBasisEnum.Currency2PerCurrency1 => fxSingleLeg.exchangeRate?.quotedCurrencyPair?.currency1?.Value
                ?? throw new InvalidOperationException("FxSingleLeg does not contain currency1"),

            QuoteBasisEnum.Currency1PerCurrency2 => fxSingleLeg.exchangeRate?.quotedCurrencyPair?.currency2?.Value
                ?? throw new InvalidOperationException("FxSingleLeg does not contain currency2"),

            _ => throw new NotImplementedException($"Unknown value for quote basis '{quoteBasis}'")
        };
    }

    private static string? GetTextIfExists(ExecutionReport message)
    {
        if (!message.IsSetText()) return null;

        return message.Text.getValue();
    }

    private static FxSwapLeg GetNearLeg(FxSwap fxSwap)
    {
        return fxSwap.nearLeg ?? throw new InvalidOperationException("nearLeg not contained in FxSwap");
    }

    private static string GetCcyFxSwap(FxSwap fxSwap)
    {
        string leadCcy;
        var leg = GetNearLeg(fxSwap);
        var quoteBasis = leg.exchangeRate?.quotedCurrencyPair?.quoteBasis;
        if (quoteBasis == QuoteBasisEnum.Currency2PerCurrency1)
        {
            leadCcy = leg.exchangeRate?.quotedCurrencyPair?.currency1?.Value ?? throw new InvalidOperationException("nearLeg does not contain currency1");
            return leadCcy;
        }
        else if (quoteBasis == QuoteBasisEnum.Currency1PerCurrency2)
        {
            leadCcy = leg.exchangeRate?.quotedCurrencyPair?.currency2?.Value ?? throw new InvalidOperationException("nearLeg does not contain currency1");
            return leadCcy;
        }
        else
        {
            throw new InvalidOperationException($"nearLeg is not contained in quoteBasis");
        }
    }

    private static TradeMatchingStatus MapStatus(char ordStatus)
    {
        return ordStatus switch
        {
            OrdStatus.NEW => TradeMatchingStatus.New,

            OrdStatus.FILLED => TradeMatchingStatus.Matched,

            OrdStatus.REJECTED => TradeMatchingStatus.Rejected,

            _ => TradeMatchingStatus.Other
        };
    }
}
