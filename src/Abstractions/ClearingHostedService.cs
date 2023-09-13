using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftWell.Fpml.Confirmation.Serialization;
using SoftWell.Fpml.Serialization;

namespace SoftWell.RtClearing;

/// <summary>
/// Фоновый сервис, который читает ордера из <see cref="IDocumentsSource" />, и если им нужен клиринг, отправляет их в <see cref="IClearingService" />
/// </summary>
public sealed class ClearingHostedService : BackgroundService
{
    private readonly IDocumentsSource _documentsSource;

    private readonly IClearingService _clearingService;

    private readonly IClearingMetaExtractor _clearingMetaExtractor;

    private readonly IDocumentSerializer _serializer;

    private readonly ILogger<ClearingHostedService> _logger;

    public ClearingHostedService(
        IDocumentsSource documentsSource,
        IClearingService clearingService,
        IClearingMetaExtractor clearingMetaExtractor,
        IDocumentSerializer serializer,
        ILogger<ClearingHostedService> logger)
    {
        _documentsSource = documentsSource ?? throw new ArgumentNullException(nameof(documentsSource));
        _clearingService = clearingService ?? throw new ArgumentNullException(nameof(clearingService));
        _clearingMetaExtractor = clearingMetaExtractor ?? throw new ArgumentNullException(nameof(clearingMetaExtractor));
        _serializer = serializer;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Клиринговый фоновый сервис запускается...");
        return base.StartAsync(cancellationToken);
    }

    public async override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Клиринговый фоновый сервис останавливается...");
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Клиринговый фоновый сервис остановился");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Клиринговый фоновый сервис запустился");
        await foreach (var doc in _documentsSource.WithCancellation(stoppingToken))
        {
            try
            {
                var meta = _clearingMetaExtractor.Extract(doc);
                var tradeId = meta.GetTradeId();
                var xml = await SerializeSafeAsync(doc, stoppingToken);

                if (_clearingMetaExtractor.IsClearingRequested(doc))
                {
                    meta = _clearingMetaExtractor.Extract(doc);
                    tradeId = meta.GetTradeId();
                    _logger.LogDebug("На клиринг пришел ордер {TradeId}", tradeId);
                    using var _ = _logger.BeginScope("TradeId={TradeId}", tradeId);
                    await _clearingService.RequestClearingAsync(meta, stoppingToken);
                }
                else _logger.LogDebug("Для ордера с номером: {TradeId} и документом {@DocumentXml} не будет производится обработка ", tradeId, xml);
            }
            catch (Exception ex)
            {
                var xml = await SerializeSafeAsync(doc, stoppingToken);
                _logger.LogError(ex, "Ошибка при обработке документа {@DocumentXml}", xml);
            }
        }
    }

    private async Task<string?> SerializeSafeAsync(Fpml.Confirmation.Document document, CancellationToken ct)
    {
        try
        {
            return await _serializer.SerializeToUtf8StringAsync(document, false, ct);
        }
        catch
        {
            return null;
        }
    }
}