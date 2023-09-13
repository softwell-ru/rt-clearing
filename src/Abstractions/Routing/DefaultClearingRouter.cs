using Microsoft.Extensions.Logging;

namespace SoftWell.RtClearing.Routing;

/// <inheritdoc />
public class DefaultClearingRouter : ClearingRouterBase
{
    private readonly IReadOnlyDictionary<string, INamedClearingService> _services;

    private readonly IClearingServiceNameProvider _clearingServiceNameProvider;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clearingServiceNameProvider"></param>
    /// <param name="services">Все зарегистрированные именованные клиринговые сервисы</param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public DefaultClearingRouter(
        IClearingServiceNameProvider clearingServiceNameProvider,
        IEnumerable<INamedClearingService> services,
        ILogger<DefaultClearingRouter> logger) : base(logger)
    {
        _services = services?.ToDictionary(x => x.Name) ?? throw new ArgumentNullException(nameof(services));
        _clearingServiceNameProvider = clearingServiceNameProvider ?? throw new ArgumentNullException(nameof(clearingServiceNameProvider));
    }

    protected override IEnumerable<INamedClearingService> GetClearingServicesForDeal(IClearingMeta meta)
    {
        ArgumentNullException.ThrowIfNull(meta);

        var serviceName = _clearingServiceNameProvider.GetClearingServiceName(meta);

        if (_services.TryGetValue(serviceName, out var result))
        {
            yield return result;
        }
    }
}
