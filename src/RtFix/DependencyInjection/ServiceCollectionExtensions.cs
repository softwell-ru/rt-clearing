using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using QuickFix;
using SoftWell.Fix.Initiator.Rt;
using SoftWell.RtClearing;
using SoftWell.RtClearing.RtFix;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRtFix(
        this IServiceCollection services,
        SessionSettings sessionSettings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(sessionSettings);

        return services.AddRtFix(
            sessionSettings,
            _ => new FileStoreFactory(sessionSettings),
            _ => new FileLogFactory(sessionSettings));
    }

    public static IServiceCollection AddRtFix(
        this IServiceCollection services,
        SessionSettings sessionSettings,
        Func<IServiceProvider, IMessageStoreFactory> messageStoreFactoryFactory,
        Func<IServiceProvider, ILogFactory> logFactoryFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(sessionSettings);

        services.AddSingleton<IRtFixClient>(sp => new RtFixClient(
           sessionSettings,
           sp.GetRequiredService<ILogger<RtFixClient>>()));

        services.TryAddSingleton<IRtFixMapper, RtFixMapper>();

        services.AddSingleton<IDocumentsSource, FixToDocumentsAdapter<IRtFixClient>>();
        services.AddSingleton<IRtTradeReporter, RtFixTradeReporter<IRtFixClient>>();

        services.AddFixClientInitiatorStarter<IRtFixClient>(
            messageStoreFactoryFactory,
            logFactoryFactory);

        return services;
    }
}