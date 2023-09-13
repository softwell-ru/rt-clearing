using Microsoft.Extensions.DependencyInjection;

namespace SoftWell.RtClearing.Routing.DependencyInjection;

public interface IRoutingBuilder
{
    IServiceCollection Services { get; }
}