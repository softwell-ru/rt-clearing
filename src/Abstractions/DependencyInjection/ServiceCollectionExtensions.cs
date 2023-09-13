using SoftWell.RtClearing;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует сервис, который в фоне читает ордера из <see cref="IDocumentsSource" /> 
    /// и отправляет их в <see cref="IClearingService" />
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddClearingProcessing(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddHostedService<ClearingHostedService>();
    }

    /// <summary>
    /// Регистрирует тип <typeparamref name="TClearingService"/> в качестве <see cref="IClearingService" />
    /// </summary>
    /// <typeparam name="TClearingService"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddClearingService<TClearingService>(
        this IServiceCollection services)
        where TClearingService : class, IClearingService
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton<IClearingService, TClearingService>();
    }

    /// <summary>
    /// Регистрирует результат <paramref name="implementationFactory"/> в качестве <see cref="IClearingService" />
    /// </summary>
    /// <param name="services"></param>
    /// <param name="implementationFactory"></param>
    /// <returns></returns>
    public static IServiceCollection AddClearingService(
        this IServiceCollection services,
        Func<IServiceProvider, IClearingService> implementationFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(implementationFactory);

        services.AddSingleton(implementationFactory);

        return services;
    }

    /// <summary>
    /// Регистрирует <paramref name="implementationObject"/> в качестве <see cref="IClearingService" />
    /// </summary>
    /// <param name="services"></param>
    /// <param name="implementationObject"></param>
    /// <returns></returns>
    public static IServiceCollection AddClearingService(
        this IServiceCollection services,
        IClearingService implementationObject)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(implementationObject);

        services.AddSingleton(implementationObject);

        return services;
    }

    /// <summary>
    /// Регистрирует тип <typeparamref name="TClearingMetaExtractor"/> в качестве <see cref="IClearingMetaExtractor" />
    /// </summary>
    /// <typeparam name="TClearingMetaExtractor"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddClearingMetaExtractor<TClearingMetaExtractor>(
        this IServiceCollection services)
        where TClearingMetaExtractor : class, IClearingMetaExtractor
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton<IClearingMetaExtractor, TClearingMetaExtractor>();
    }

    /// <summary>
    /// Регистрирует результат <paramref name="implementationFactory" /> в качестве <see cref="IClearingMetaExtractor" />
    /// </summary>
    /// <param name="services"></param>
    /// <param name="implementationFactory"></param>
    /// <returns></returns>
    public static IServiceCollection AddClearingMetaExtractor(
        this IServiceCollection services,
        Func<IServiceProvider, IClearingMetaExtractor> implementationFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(implementationFactory);

        services.AddSingleton(implementationFactory);

        return services;
    }

    /// <summary>
    /// Регистрирует <paramref name="implementationObject" /> в качестве <see cref="IClearingMetaExtractor" />
    /// </summary>
    /// <param name="services"></param>
    /// <param name="implementationObject"></param>
    /// <returns></returns>
    public static IServiceCollection AddClearingMetaExtractor(
        this IServiceCollection services,
        IClearingMetaExtractor implementationObject)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(implementationObject);

        services.AddSingleton(implementationObject);

        return services;
    }

    /// <summary>
    /// Регистрирует <see cref="DefaultClearingMetaExtractor" /> в качестве <see cref="IClearingMetaExtractor" />
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDefaultClearingMetaExtractor(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddClearingMetaExtractor<DefaultClearingMetaExtractor>();

        return services;
    }
}