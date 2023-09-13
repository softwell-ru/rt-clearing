using SoftWell.Fix.Initiator;

namespace SoftWell.RtClearing.RtFix;

public class RtFixTradeReporter<TFixMessagesSender> : IRtTradeReporter
    where TFixMessagesSender : IFixMessagesSender
{
    private readonly TFixMessagesSender _fixMessagesSender;

    private readonly IRtFixMapper _rtFixMapper;

    public RtFixTradeReporter(
        TFixMessagesSender fixMessagesSender,
        IRtFixMapper rtFixMapper)
    {
        _fixMessagesSender = fixMessagesSender ?? throw new ArgumentNullException(nameof(fixMessagesSender));
        _rtFixMapper = rtFixMapper ?? throw new ArgumentNullException(nameof(rtFixMapper));
    }

    public async Task ReportStatusChangedAsync(TradeMatchingStatusChanged status, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(status);

        var report = _rtFixMapper.MapToExecutionReport(status);

        await _fixMessagesSender.SendMessageAsync(report, ct);
    }
}
