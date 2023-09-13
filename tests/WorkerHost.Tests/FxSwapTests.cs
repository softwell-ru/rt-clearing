using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFix.Fields;
using QuickFix.FIX50SP2;
using SoftWell.Fpml.Confirmation.Serialization;
using SoftWell.Fpml.Serialization;
using SoftWell.RtClearing.WorkerHost.Tests.Infrastructure;

namespace SoftWell.RtClearing.WorkerHost.Tests;

[TestClass]
public class FxSwapTests
{
    [TestMethod]
    public async Task Test()
    {
        await using var f = new AppFactory();

        var serializer = f.Resolve<IDocumentSerializer>();

        var awaiter = f.MoexFixClientMock.WaitForOutgoingMessageAsync<NewOrderSingle>(
            NewOrderSingle.MsgType,
            x => x.IsSetSymbol() && x.Symbol.getValue() == "USD000TODTOM"
            && x.IsSetSide() && x.Side.getValue() == QuickFix.Fields.Side.SELL
            && x.IsSetOrderQty() && x.OrderQty.getValue() == 10000
            && x.IsSetOrdType() && x.OrdType.getValue() == OrdType.FOREX_SWAP);

        f.DocumentsSource.EmulateDocument(serializer.DeserializeFromUtf8String(_buyRubFromParty1FxSwap));

        await awaiter.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static readonly string _buyRubFromParty1FxSwap = $"""
<?xml version="1.0" encoding="utf-8"?>
<dataDocument xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" actualBuild="8" xmlns="http://www.fpml.org/FpML-5/confirmation">
	<onBehalfOf>
		<partyReference href="Party1137"/>
	</onBehalfOf>
	<trade>
		<tradeHeader>
			<partyTradeIdentifier>
				<partyReference href="Party1137"/>
				<accountReference href="Party1Account137"/>
				<tradeId tradeIdScheme="https://hihiclub.ru/coding-schemes/trade-id">1534688</tradeId>
			</partyTradeIdentifier>
			<partyTradeIdentifier>
				<partyReference href="Party2137"/>
				<tradeId tradeIdScheme="https://hihiclub.ru/coding-schemes/trade-id">1534688</tradeId>
			</partyTradeIdentifier>
			<partyTradeInformation>
				<partyReference href="Party1137"/>
				<relatedParty>
					<partyReference href="Centralparty31"/>
					<role>ClearingOrganization</role>
				</relatedParty>
				<trader traderScheme="https://hihiclub.ru/coding-schemes/trader">SOFT_SOFT_SOFT 1</trader>
			</partyTradeInformation>
			<partyTradeInformation>
				<partyReference href="Party2137"/>
				<relatedParty>
					<partyReference href="Centralparty31"/>
					<role>ClearingOrganization</role>
				</relatedParty>
			</partyTradeInformation>
			<tradeDate>2023-08-29</tradeDate>
			<clearedDate>2023-08-29</clearedDate>
		</tradeHeader>
		<fxSwap>
			<productId productIdScheme="https://hihiclub.ru/coding-schemes/instrument-id">FXS-USD-RUB-ON</productId>
			<productId productIdScheme="https://hihiclub.ru/coding-schemes/instrument-id">FXS-USD-RUB-ON</productId>
			<nearLeg>
				<exchangedCurrency1>
					<payerPartyReference href="Party1137"/>
					<receiverPartyReference href="Party2137"/>
					<paymentAmount>
						<currency currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">USD</currency>
						<amount>10000</amount>
					</paymentAmount>
					<paymentDate>
						<dateAdjustments>
							<businessDayConvention>MODFOLLOWING</businessDayConvention>
							<businessCenters>
								<businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">RUS</businessCenter>
								<businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">NY</businessCenter>
							</businessCenters>
						</dateAdjustments>
						<unadjustedDate>2023-08-29</unadjustedDate>
					</paymentDate>
				</exchangedCurrency1>
				<exchangedCurrency2>
					<payerPartyReference href="Party2137"/>
					<receiverPartyReference href="Party1137"/>
					<paymentAmount>
						<currency currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">RUB</currency>
						<amount>1010000</amount>
					</paymentAmount>
					<paymentDate>
						<dateAdjustments>
							<businessDayConvention>MODFOLLOWING</businessDayConvention>
							<businessCenters>
								<businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">RUS</businessCenter>
								<businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">NY</businessCenter>
							</businessCenters>
						</dateAdjustments>
						<unadjustedDate>2023-08-29</unadjustedDate>
					</paymentDate>
				</exchangedCurrency2>
				<dealtCurrency>ExchangedCurrency1</dealtCurrency>
				<tenorName>Broken</tenorName>
				<tenorPeriod>
					<periodMultiplier>0</periodMultiplier>
					<period>D</period>
				</tenorPeriod>
				<valueDate>2023-08-29</valueDate>
				<exchangeRate>
					<quotedCurrencyPair>
						<currency1 currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">USD</currency1>
						<currency2 currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">RUB</currency2>
						<quoteBasis>Currency2PerCurrency1</quoteBasis>
					</quotedCurrencyPair>
					<rate>101</rate>
				</exchangeRate>
			</nearLeg>
			<farLeg>
				<exchangedCurrency1>
					<payerPartyReference href="Party2137"/>
					<receiverPartyReference href="Party1137"/>
					<paymentAmount>
						<currency currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">USD</currency>
						<amount>10000</amount>
					</paymentAmount>
					<paymentDate>
						<dateAdjustments>
							<businessDayConvention>MODFOLLOWING</businessDayConvention>
							<businessCenters>
								<businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">RUS</businessCenter>
								<businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">NY</businessCenter>
							</businessCenters>
						</dateAdjustments>
						<unadjustedDate>2023-08-30</unadjustedDate>
					</paymentDate>
				</exchangedCurrency1>
				<exchangedCurrency2>
					<payerPartyReference href="Party1137"/>
					<receiverPartyReference href="Party2137"/>
					<paymentAmount>
						<currency currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">RUB</currency>
						<amount>1010100</amount>
					</paymentAmount>
					<paymentDate>
						<dateAdjustments>
							<businessDayConvention>MODFOLLOWING</businessDayConvention>
							<businessCenters>
								<businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">RUS</businessCenter>
								<businessCenter businessCenterScheme="https://hihiclub.ru/coding-schemes/business-center">NY</businessCenter>
							</businessCenters>
						</dateAdjustments>
						<unadjustedDate>2023-08-30</unadjustedDate>
					</paymentDate>
				</exchangedCurrency2>
				<dealtCurrency>ExchangedCurrency1</dealtCurrency>
				<tenorName>Broken</tenorName>
				<tenorPeriod>
					<periodMultiplier>1</periodMultiplier>
					<period>D</period>
				</tenorPeriod>
				<valueDate>2023-08-30</valueDate>
				<exchangeRate>
					<quotedCurrencyPair>
						<currency1 currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">USD</currency1>
						<currency2 currencyScheme="http://www.fpml.org/coding-scheme/external/iso4217-2001-08-15">RUB</currency2>
						<quoteBasis>Currency2PerCurrency1</quoteBasis>
					</quotedCurrencyPair>
					<rate>101.01</rate>
				</exchangeRate>
			</farLeg>
		</fxSwap>
	</trade>
	<party id="Party1137">
		<partyId partyIdScheme="https://hihiclub.ru/coding-schemes/partner">SOFT</partyId>
	</party>
	<party id="Party2137">
		<partyId partyIdScheme="https://hihiclub.ru/coding-schemes/partner">TEST</partyId>
	</party>
	<party id="Centralparty31">
		<partyId partyIdScheme="https://hihiclub.ru/coding-schemes/partner">MOEX</partyId>
	</party>
	<account id="Party1Account137">
		<accountId accountIdScheme="https://hihiclub.ru/coding-schemes/account-id">SOFT</accountId>
	</account>
</dataDocument>
""";
}