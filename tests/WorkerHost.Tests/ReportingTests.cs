using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX50SP2;
using SoftWell.RtClearing.RtFix;
using SoftWell.RtClearing.Testing;
using SoftWell.RtClearing.WorkerHost.Tests.Infrastructure;

namespace SoftWell.RtClearing.WorkerHost.Tests;

[TestClass]
public class ReportingTests
{
    [DataTestMethod]
    [DataRow(OrdStatus.NEW, null, TradeMatchingStatus.New)]
    [DataRow(OrdStatus.FILLED, "deal matched", TradeMatchingStatus.Matched)]
    [DataRow(OrdStatus.REJECTED, "deal rejected", TradeMatchingStatus.Rejected)]
    [DataRow(OrdStatus.PARTIALLY_FILLED, "something", TradeMatchingStatus.Other)]
    [DataRow(OrdStatus.CANCELED, "something else", TradeMatchingStatus.Other)]
    public async Task When_MoexClearingService_Receives_ExecutionReport_Report_TradeMatchingStatusChanged(char moexStatus, string? text, TradeMatchingStatus expectedStatus)
    {
        var testReporter = new TestRtTradeReporter();

        await using var f = new AppFactory(services =>
        {
            services.AddSingleton(testReporter.Object);
        });

        var tradeId = Guid.NewGuid().ToString();
        var externalTradeId = Guid.NewGuid().ToString();

        var awaiter = testReporter.WaitForTradeStatusChangedAsync(
            x => x.ExternalSystem == "MOEX"
                && x.TradeId == tradeId
                & x.ExternalTradeId == externalTradeId
                && x.Status == expectedStatus
                && x.Comment == text
                && x.TradingSessionId == "CPCL");

        var report = new ExecutionReport
        {
            ClOrdID = new ClOrdID(tradeId),
            OrdStatus = new OrdStatus(moexStatus),
            OrderID = new OrderID(externalTradeId),
            TransactTime = new TransactTime(DateTime.UtcNow)
        };

        if (text is not null)
        {
            report.Text = new Text(text);
        }

        f.MoexFixClientMock.EmulateIncomingMessage(report);

        await awaiter.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [DataTestMethod]
    [DataRow(TradeMatchingStatus.New, null, OrdStatus.NEW, true)]
    [DataRow(TradeMatchingStatus.Matched, "deal matched", RtFixConstants.OrdStatus.MATCHED, false)]
    [DataRow(TradeMatchingStatus.Rejected, "deal rejected", OrdStatus.REJECTED, true)]
    [DataRow(TradeMatchingStatus.Other, "something", RtFixConstants.OrdStatus.OTHER, false)]
    public async Task When_TradeMatchingStatusChanged_Reported_RtFixClient_Should_Send_ExecutionReport(
        TradeMatchingStatus reportedStatus,
        string? comment,
        char expectedRtStatus,
        bool hasExternalTradeId)
    {
        await using var f = new AppFactory();

        var reporter = f.Resolve<IRtTradeReporter>();

        var tradeId = Guid.NewGuid().ToString();
        var externalTradeId = hasExternalTradeId ? Guid.NewGuid().ToString() : null;
        var externalSystem = Guid.NewGuid().ToString();
        var tradingSessionId = Guid.NewGuid().ToString();

        var awaiter = f.RtFixClientMock.WaitForOutgoingMessageAsync<ExecutionReport>(
            ExecutionReport.MsgType,
            x => x.IsSetClOrdID()
                && x.IsSetTransactTime()
                && x.IsSetExecType() && x.ExecType.getValue() == RtFixConstants.ExecType.MATCHING_STATUS
                && x.IsSetOrderID() && x.OrderID.getValue() == tradeId
                &&
                    (
                        (hasExternalTradeId && x.IsSetSecondaryOrderID() && x.SecondaryOrderID.getValue() == externalTradeId)
                        ||
                        (!hasExternalTradeId && !x.IsSetSecondaryOrderID())
                    )
                && x.IsSetOrdStatus() && x.OrdStatus.getValue() == expectedRtStatus
                &&
                    (
                        (comment is not null && x.IsSetText() && x.Text.getValue() == comment)
                        ||
                        (comment is null && !x.IsSetText())
                    )
                && x.IsSetTradingSessionID() && x.TradingSessionID.getValue() == tradingSessionId
                && x.IsSetNoPartyIDs() && x.NoPartyIDs.getValue() == 1
                && x.HasGroup<ExecutionReport.NoPartyIDsGroup>(
                    1,
                    gr => gr.IsSetPartyID() && gr.PartyID.getValue() == externalSystem
                        && gr.IsSetPartyIDSource() && gr.PartyIDSource.getValue() == PartyIDSource.PROPRIETARY_CUSTOM_CODE
                        && gr.IsSetPartyRole() && gr.PartyRole.getValue() == RtFixConstants.PartyRole.EXTERNAL_SYSTEM));

        await reporter.ReportStatusChangedAsync(
            new TradeMatchingStatusChanged
            {
                TimestampUtc = DateTime.UtcNow,
                TradeId = tradeId,
                ExternalTradeId = externalTradeId,
                ExternalSystem = externalSystem,
                Status = reportedStatus,
                TradingSessionId = tradingSessionId,
                Comment = comment
            });

        await awaiter.WaitAsync(TimeSpan.FromSeconds(5));
    }
}
