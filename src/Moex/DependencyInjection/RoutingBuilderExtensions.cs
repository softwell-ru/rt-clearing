using QuickFix;
using SoftWell.RtClearing.Moex;
using SoftWell.RtClearing.Moex.Configuration;
using SoftWell.RtClearing.Routing.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class RoutingBuilderExtensions
{
    public static IRoutingBuilder AddNamedMoexClearingService(
        this IRoutingBuilder builder,
        string name,
        MoexClearingOptions options,
        SessionSettings sessionSettings)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(sessionSettings);

        return builder.AddNamedMoexClearingService(
            name,
            options,
            sessionSettings,
            _ => Helpers.GetDefaultMessageFactory(sessionSettings),
            _ => Helpers.GetDefaultLogFactory(sessionSettings));
    }

    public static IRoutingBuilder AddNamedMoexClearingService(
        this IRoutingBuilder builder,
        string name,
        MoexClearingOptions options,
        SessionSettings sessionSettings,
        Func<IServiceProvider, IMessageStoreFactory> messageStoreFactoryFactory,
        Func<IServiceProvider, ILogFactory> logFactoryFactory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        builder.Services.AddMoexClearingServiceCore(options, sessionSettings, messageStoreFactoryFactory, logFactoryFactory);
        builder.AddNamedClearingService<MoexClearingService>(name);
        return builder;
    }
}