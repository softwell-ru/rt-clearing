namespace SoftWell.RtClearing.WorkerHost.Dummy;

public class LogWriterRtTradeReporter : IRtTradeReporter
{
    private readonly ILogger _logger;

    public LogWriterRtTradeReporter(ILogger<LogWriterRtTradeReporter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReportStatusChangedAsync(TradeMatchingStatusChanged status, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(status);

        _logger.LogInformation("Ордер изменил статус: {@TradeStatus}", status);
        return Task.CompletedTask;
    }
}
