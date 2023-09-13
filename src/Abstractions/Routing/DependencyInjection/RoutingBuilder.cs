using Microsoft.Extensions.DependencyInjection;

namespace SoftWell.RtClearing.Routing.DependencyInjection;

internal sealed class RoutingBuilder : IRoutingBuilder
{
    public RoutingBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IServiceCollection Services { get; }
}
