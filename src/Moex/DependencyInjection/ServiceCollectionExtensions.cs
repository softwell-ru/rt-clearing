using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.FIX50SP2;
using SoftWell.RtClearing;
using SoftWell.RtClearing.Moex;
using SoftWell.RtClearing.Moex.Configuration;
using SoftWell.RtCodes;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMoexClearingService(
        this IServiceCollection services,
        MoexClearingOptions options,
        SessionSettings sessionSettings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(sessionSettings);

        return services.AddMoexClearingService(
            options,
            sessionSettings,
            _ => Helpers.GetDefaultMessageFactory(sessionSettings),
            _ => Helpers.GetDefaultLogFactory(sessionSettings));
    }

    public static IServiceCollection AddMoexClearingService(
        this IServiceCollection services,
        MoexClearingOptions options,
        SessionSettings sessionSettings,
        Func<IServiceProvider, IMessageStoreFactory> messageStoreFactoryFactory,
        Func<IServiceProvider, ILogFactory> logFactoryFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(sessionSettings);

        services.AddMoexClearingServiceCore(options, sessionSettings, messageStoreFactoryFactory, logFactoryFactory);
        services.AddClearingService<MoexClearingService>();

        return services;
    }

    internal static IServiceCollection AddMoexClearingServiceCore(
        this IServiceCollection services,
        MoexClearingOptions options,
        SessionSettings sessionSettings,
        Func<IServiceProvider, IMessageStoreFactory> messageStoreFactoryFactory,
        Func<IServiceProvider, ILogFactory> logFactoryFactory)
    {
        Helpers.FixSessionSettings(sessionSettings);

        services.AddSingleton<IMoexFixClient>(sp => new MoexFixClient(
            sessionSettings,
            sp.GetRequiredService<ILogger<MoexFixClient>>()));

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMoexFixClient>();
            return new MoexClearingService(
                options,
                client,
                sp.GetRequiredService<ICodesConverter>(),
                sp.GetRequiredService<IRtTradeReporter>(),
                sp.GetRequiredService<ILogger<MoexClearingService>>());
        });

        services.AddFixMessagesHandling<IMoexFixClient>(
            opts => opts.AddMessagesHandler<ExecutionReport, MoexClearingService>());

        services.AddFixClientInitiatorStarter<IMoexFixClient>(
            messageStoreFactoryFactory,
            logFactoryFactory);

        return services;
    }
}