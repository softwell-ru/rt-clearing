using SoftWell.RtClearing;
using SoftWell.RtClearing.Routing;
using SoftWell.RtClearing.Routing.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class RoutingServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует <see cref="DefaultClearingRouter" /> в качестве <see cref="IClearingService" />
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddDefaultClearingRouting(
        this IServiceCollection services,
        Action<IRoutingBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        return services.AddClearingRouting<DefaultClearingRouter>(configure);
    }

    /// <summary>
    /// Регистрирует <typeparamref name="TClearingRouter" /> в качестве <see cref="IClearingService" />
    /// </summary>
    /// <typeparam name="TClearingRouter"></typeparam>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddClearingRouting<TClearingRouter>(
        this IServiceCollection services,
        Action<IRoutingBuilder> configure)
        where TClearingRouter : ClearingRouterBase
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddSingleton<IClearingService, TClearingRouter>();

        var builder = new RoutingBuilder(services);
        builder.AddClearingServiceNameProvider<PartyIdClearingServiceNameProvider>();
        configure(builder);
        return services;
    }
}