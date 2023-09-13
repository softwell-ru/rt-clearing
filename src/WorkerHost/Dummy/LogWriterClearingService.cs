using SoftWell.Fpml.Confirmation.Serialization;
using SoftWell.Fpml.Serialization;

namespace SoftWell.RtClearing.WorkerHost.Dummy;

/// <summary>
/// Ничего не делает, просто пишет в лог
/// </summary>
public class LogWriterClearingService : IClearingService
{
    private readonly IDocumentSerializer _serializer;

    private readonly ILogger<LogWriterClearingService> _logger;

    public LogWriterClearingService(
        IDocumentSerializer serializer,
        ILogger<LogWriterClearingService> logger)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RequestClearingAsync(IClearingMeta meta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(meta);

        var str = await _serializer.SerializeToUtf8StringAsync(meta.Document, true, ct);

        _logger.LogInformation("На клиринг пришел документ {ClearingDocumentXml}", str);
    }
}
