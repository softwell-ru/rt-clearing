using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX50SP2;

namespace SoftWell.RtClearing.RtFix;

public class RtFixMapper : IRtFixMapper
{
    public ExecutionReport MapToExecutionReport(TradeMatchingStatusChanged status)
    {
        ArgumentNullException.ThrowIfNull(status);
        if (status.Status == TradeMatchingStatus.None) throw new ArgumentException("Status field should not have value 'None'", nameof(status));

        var report = new ExecutionReport
        {
            ClOrdID = new ClOrdID(Guid.NewGuid().ToString()),
            TransactTime = new TransactTime(status.TimestampUtc),
            OrderID = new OrderID(status.TradeId),
            ExecType = new ExecType(RtFixConstants.ExecType.MATCHING_STATUS),
            TradingSessionID = new TradingSessionID(status.TradingSessionId),
            OrdStatus = new OrdStatus(MapToOrdStatus(status.Status))
        };

        if (status.ExternalTradeId is not null)
        {
            report.SecondaryOrderID = new SecondaryOrderID(status.ExternalTradeId);
        }

        if (status.Comment is not null)
        {
            report.Text = new Text(status.Comment);
        }

        report.AddGroup(new ExecutionReport.NoPartyIDsGroup
        {
            PartyID = new PartyID(status.ExternalSystem),
            PartyIDSource = new PartyIDSource(PartyIDSource.PROPRIETARY_CUSTOM_CODE),
            PartyRole = new PartyRole(RtFixConstants.PartyRole.EXTERNAL_SYSTEM)
        });

        return report;
    }

    public TradeMatchingStatusChanged MapToTradeMatchingStatusChanged(ExecutionReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (!report.IsSetClOrdID()) throw new ArgumentException("ClOrdID should be present", nameof(report));
        if (!report.IsSetTransactTime()) throw new ArgumentException("TransactTime should be present", nameof(report));
        if (!report.IsSetExecType() || report.ExecType.getValue() != RtFixConstants.ExecType.MATCHING_STATUS) throw new ArgumentException($"ExecType should be present and have value '{RtFixConstants.ExecType.MATCHING_STATUS}'", nameof(report));
        if (!report.IsSetOrderID()) throw new ArgumentException("OrderID should be present", nameof(report));
        if (!report.IsSetOrdStatus()) throw new ArgumentException("OrdStatus should be present", nameof(report));
        if (!report.IsSetTradingSessionID()) throw new ArgumentException("TradingSessionID should be present", nameof(report));
        if (!report.IsSetNoPartyIDs() || report.NoPartyIDs.getValue() != 1) throw new ArgumentException("NoPartyIDs should be present and have value '1'", nameof(report));

        var party = report.GetGroup<ExecutionReport.NoPartyIDsGroup>(1);

        if (!party.IsSetPartyID()) throw new ArgumentException("PartyID should be present", nameof(report));
        if (!party.IsSetPartyIDSource() || party.PartyIDSource.getValue() != PartyIDSource.PROPRIETARY_CUSTOM_CODE) throw new ArgumentException($"PartyIDSource should be present and have value '{PartyIDSource.PROPRIETARY_CUSTOM_CODE}'", nameof(report));
        if (!party.IsSetPartyRole() || party.PartyRole.getValue() != RtFixConstants.PartyRole.EXTERNAL_SYSTEM) throw new ArgumentException($"PartyRole should be present and have value '{RtFixConstants.PartyRole.EXTERNAL_SYSTEM}'", nameof(report));

        var res = new TradeMatchingStatusChanged
        {
            TimestampUtc = report.TransactTime.getValue(),
            TradeId = report.OrderID.getValue(),
            ExternalTradeId = report.IsSetSecondaryOrderID() ? report.SecondaryOrderID.getValue() : null,
            TradingSessionId = report.TradingSessionID.getValue(),
            ExternalSystem = party.PartyID.getValue(),
            Comment = report.IsSetText() ? report.Text.getValue() : null,
            Status = MapToTradeMatchingStatus(report.OrdStatus.getValue())
        };

        return res;
    }

    private static char MapToOrdStatus(TradeMatchingStatus status)
    {
        return status switch
        {
            TradeMatchingStatus.New => OrdStatus.NEW,

            TradeMatchingStatus.Matched => RtFixConstants.OrdStatus.MATCHED,

            TradeMatchingStatus.Rejected => OrdStatus.REJECTED,

            TradeMatchingStatus.Other => RtFixConstants.OrdStatus.OTHER,

            _ => throw new NotImplementedException($"Trade matching status '{status}' is not supported")
        };
    }

    private static TradeMatchingStatus MapToTradeMatchingStatus(char ordStatus)
    {
        return ordStatus switch
        {
            OrdStatus.NEW => TradeMatchingStatus.New,

            RtFixConstants.OrdStatus.MATCHED => TradeMatchingStatus.Matched,

            OrdStatus.REJECTED => TradeMatchingStatus.Rejected,

            RtFixConstants.OrdStatus.OTHER => TradeMatchingStatus.Other,

            _ => throw new NotImplementedException($"OrdStatus '{ordStatus}' is not supported")
        };
    }
}
