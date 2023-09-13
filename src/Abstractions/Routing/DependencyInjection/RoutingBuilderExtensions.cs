using Microsoft.Extensions.DependencyInjection.Extensions;
using SoftWell.RtClearing;
using SoftWell.RtClearing.Routing;
using SoftWell.RtClearing.Routing.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class RoutingBuilderExtensions
{
    /// <summary>
    /// Регистрирует результат <paramref name="implementationFactory"/> в качестве именованного <see cref="IClearingService" />
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <param name="implementationFactory"></param>
    /// <returns></returns>
    public static IRoutingBuilder AddNamedClearingService(
        this IRoutingBuilder builder,
        string name,
        Func<IServiceProvider, IClearingService> implementationFactory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(implementationFactory);

        builder.Services.AddSingleton<INamedClearingService>(
            sp => new NamedClearingService(name, implementationFactory(sp)));

        return builder;
    }

    /// <summary>
    /// Регистрирует <paramref name="implementationObject"/> в качестве именованного <see cref="IClearingService" />
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <param name="implementationObject"></param>
    /// <returns></returns>
    public static IRoutingBuilder AddNamedClearingService(
        this IRoutingBuilder builder,
        string name,
        IClearingService implementationObject)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(implementationObject);

        builder.Services.AddSingleton<INamedClearingService>(
            new NamedClearingService(name, implementationObject));

        return builder;
    }

    /// <summary>
    /// Регистрирует тип <typeparamref name="TClearingService"/> в качестве именованного <see cref="IClearingService" />
    /// </summary>
    /// <typeparam name="TClearingService"></typeparam>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IRoutingBuilder AddNamedClearingService<TClearingService>(
        this IRoutingBuilder builder,
        string name)
        where TClearingService : class, IClearingService
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        builder.Services.TryAddSingleton<TClearingService>();

        builder.Services.AddSingleton<INamedClearingService>(
            sp => new NamedClearingService(
                name,
                sp.GetRequiredService<TClearingService>()));

        return builder;
    }

    /// <summary>
    /// Регистрирует результат <paramref name="implementationFactory"/> в качестве <see cref="IClearingServiceNameProvider" />
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="implementationFactory"></param>
    /// <returns></returns>
    public static IRoutingBuilder AddClearingServiceNameProvider(
        this IRoutingBuilder builder,
        Func<IServiceProvider, IClearingServiceNameProvider> implementationFactory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(implementationFactory);

        builder.Services.AddSingleton(implementationFactory);

        return builder;
    }

    /// <summary>
    /// Регистрирует <paramref name="implementationObject"/> в качестве <see cref="IClearingServiceNameProvider" />
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="implementationFactory"></param>
    /// <returns></returns>
    public static IRoutingBuilder AddClearingServiceNameProvider(
        this IRoutingBuilder builder,
        IClearingServiceNameProvider implementationObject)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(implementationObject);

        builder.Services.AddSingleton(implementationObject);

        return builder;
    }

    /// <summary>
    /// Регистрирует тип <typeparamref name="TClearingServiceNameProvider"/> в качестве <see cref="IClearingServiceNameProvider" />
    /// </summary>
    /// <typeparam name="TClearingServiceNameProvider"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IRoutingBuilder AddClearingServiceNameProvider<TClearingServiceNameProvider>(
        this IRoutingBuilder builder)
        where TClearingServiceNameProvider : class, IClearingServiceNameProvider
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton<IClearingServiceNameProvider, TClearingServiceNameProvider>();

        return builder;
    }
}