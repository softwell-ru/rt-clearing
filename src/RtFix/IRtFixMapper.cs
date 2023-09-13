using QuickFix.FIX50SP2;

namespace SoftWell.RtClearing.RtFix;

public interface IRtFixMapper
{
    ExecutionReport MapToExecutionReport(TradeMatchingStatusChanged status);

    TradeMatchingStatusChanged MapToTradeMatchingStatusChanged(ExecutionReport report);
}
