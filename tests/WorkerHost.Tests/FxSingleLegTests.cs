using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFix.FIX50SP2;
using SoftWell.Fpml.Confirmation.Serialization;
using SoftWell.Fpml.Serialization;
using SoftWell.RtClearing.WorkerHost.Tests.Infrastructure;

namespace SoftWell.RtClearing.WorkerHost.Tests;

[TestClass]
public class FxSingleLegTests
{
    [TestMethod]
    public async Task Test()
    {
        await using var f = new AppFactory();

        var serializer = f.Resolve<IDocumentSerializer>();

        var awaiter = f.MoexFixClientMock.WaitForOutgoingMessageAsync<NewOrderSingle>(
            NewOrderSingle.MsgType,
            x => x.IsSetSymbol() && x.Symbol.getValue() == "USD000UTSTOM"
                && x.IsSetSide() && x.Side.getValue() == QuickFix.Fields.Side.BUY
                && x.IsSetOrderQty() && x.OrderQty.getValue() == 2);

        f.DocumentsSource.EmulateDocument(serializer.DeserializeFromUtf8String(_buyRubFromParty1));

        await awaiter.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static readonly string _tradeIdForParty1 = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    private const int _amount = 2;

    private const int _price = 86;

    private static readonly string _buyRubFromParty1 = $"""
<?xml version="1.0" encoding="utf-8"?>
<dataDocument xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" actualBuild="8" xmlns="http://www.fpml.org/FpML-5/confirmation">
  <onBehalfOf>
    <partyReference href="Party13" />
  </onBehalfOf>
  <trade>
    <tradeHeader>
      <partyTradeIdentifier>
        <partyReference href="Party13" />
        <accountReference href="Party1Account3" />
        <tradeId tradeIdScheme="https://hihiclub.ru/coding-schemes/trade-id">{_tradeIdForParty1}</tradeId>
      </partyTradeIdentifier>
      <partyTradeIdentifier>
        <partyReference href="Party23" />
        <tradeId tradeIdScheme="https://hihiclub.ru/coding-schemes/trade-id">{_tradeIdForParty1}</tradeId>
      </partyTradeIdentifier>
      <partyTradeInformation>
        <partyReference href="Party13" />
        <relatedParty>
          <partyReference href="Centralparty" />
          <role>ClearingOrganization</role>
        </relatedParty>
      </partyTradeInformation>
      <partyTradeInformation>
        <partyReference href="Party23" />
        <relatedParty>
          <partyReference href="Centralparty" />
          <role>ClearingOrganization</role>
        </relatedParty>
      </partyTradeInformation>
      <tradeDate>2025-06-02</tradeDate>
      <clearedDate>2025-06-02</clearedDate>
    </tradeHeader>
    <fxSingleLeg>
      <productType productTypeScheme="https://hihiclub.ru/coding-schemes/product-type">FX</productType>
      <productId productIdScheme="https://hihiclub.ru/coding-schemes/instrument-id">FX-USD-RUB-TOM</productId>
      <exchangedCurrency1>
        <payerPartyReference href="Party23" />
        <receiverPartyReference href="Party13" />
        <paymentAmount>
          <currency currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">USD</currency>
          <amount>{_amount}</amount>
        </paymentAmount>
        <paymentDate>
          <dateAdjustments>
            <businessDayConvention>MODFOLLOWING</businessDayConvention>
            <businessCenters>
              <businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">RUS</businessCenter>
              <businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">NY</businessCenter>
            </businessCenters>
          </dateAdjustments>
          <unadjustedDate>2025-06-02</unadjustedDate>
        </paymentDate>
      </exchangedCurrency1>
      <exchangedCurrency2>
        <payerPartyReference href="Party13" />
        <receiverPartyReference href="Party23" />
        <paymentAmount>
          <currency currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">RUB</currency>
          <amount>{_price * _amount}</amount>
        </paymentAmount>
        <paymentDate>
          <dateAdjustments>
            <businessDayConvention>MODFOLLOWING</businessDayConvention>
            <businessCenters>
              <businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">RUS</businessCenter>
              <businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">NY</businessCenter>
            </businessCenters>
          </dateAdjustments>
          <unadjustedDate>2025-06-02</unadjustedDate>
        </paymentDate>
      </exchangedCurrency2>
      <dealtCurrency>ExchangedCurrency1</dealtCurrency>
      <valueDate>2025-06-02</valueDate>
      <exchangeRate>
        <quotedCurrencyPair>
          <currency1 currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">USD</currency1>
          <currency2 currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">RUB</currency2>
          <quoteBasis>Currency2PerCurrency1</quoteBasis>
        </quotedCurrencyPair>
        <rate>{_price}</rate>
      </exchangeRate>
    </fxSingleLeg>
  </trade>
  <party id="Party13">
    <partyId partyIdScheme="https://hihiclub.ru/coding-schemes/partner">SOFT</partyId>
  </party>
  <party id="Party23">
    <partyId partyIdScheme="https://hihiclub.ru/coding-schemes/partner">TEST</partyId>
  </party>
  <party id="Centralparty">
    <partyId partyIdScheme="https://hihiclub.ru/coding-schemes/partner">MOEX</partyId>
  </party>
  <account id="Party1Account3" />
</dataDocument>
""";
}