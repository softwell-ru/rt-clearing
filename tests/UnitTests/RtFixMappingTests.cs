using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftWell.RtClearing.RtFix;

namespace SoftWell.RtClearing.UnitTests;

[TestClass]
public class RtFixMappingTests
{
    [DataTestMethod]
    [DataRow(TradeMatchingStatus.New, null, true)]
    [DataRow(TradeMatchingStatus.Matched, "deal matched", false)]
    [DataRow(TradeMatchingStatus.Rejected, "deal rejected", true)]
    [DataRow(TradeMatchingStatus.Other, "something", false)]
    public void Should_Map_To_ExecutionReport_And_Back(
        TradeMatchingStatus reportedStatus,
        string? comment,
        bool hasExternalTradeId)
    {
        var mapper = new RtFixMapper();

        var tradeId = Guid.NewGuid().ToString();
        var externalTradeId = hasExternalTradeId ? Guid.NewGuid().ToString() : null;
        var externalSystem = Guid.NewGuid().ToString();
        var tradingSessionId = Guid.NewGuid().ToString();

        var status = new TradeMatchingStatusChanged
        {
            TimestampUtc = DateTime.UtcNow,
            TradeId = tradeId,
            ExternalTradeId = externalTradeId,
            ExternalSystem = externalSystem,
            Status = reportedStatus,
            TradingSessionId = tradingSessionId,
            Comment = comment
        };

        var report = mapper.MapToExecutionReport(status);
        var status2 = mapper.MapToTradeMatchingStatusChanged(report);

        Assert.AreEqual(status, status2);
    }
}