using Microsoft.Extensions.Logging;

namespace SoftWell.RtClearing.Routing;

/// <summary>
/// Абстрактный роутер для отправки ордеров в разные клиринговые организации.
/// Для каждого ордера получает набор клиринговых сервисов, и отправляет в них ордер
/// </summary>
public abstract class ClearingRouterBase : IClearingService
{
    private readonly ILogger _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clearingServiceNameProvider"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ClearingRouterBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RequestClearingAsync(IClearingMeta meta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(meta);

        var services = GetClearingServicesForDeal(meta).ToList();

        if (services.Count == 0)
        {
            _logger.LogError("Не удалось получить клиринговые сервисы для ордера.");
            throw new UnknownClearingServiceException(meta.GetTradeId());
        }

        _logger.LogDebug("Ордер будет обрабатываться сервисами ['{@ClearingServiceNames}']", services.Select(x => x.Name));

        foreach (var srv in services)
        {
            await srv.RequestClearingAsync(meta, ct);
        }
    }

    protected abstract IEnumerable<INamedClearingService> GetClearingServicesForDeal(IClearingMeta meta);
}