namespace SoftWell.RtClearing;

/// <summary>
/// Оповещатель RuTerminal об ордерах
/// </summary>
public interface IRtTradeReporter
{
    Task ReportStatusChangedAsync(TradeMatchingStatusChanged status, CancellationToken ct = default);
}
