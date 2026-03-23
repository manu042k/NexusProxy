using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NexusProxy.Core.Configuration;
using NexusProxy.Core.Interfaces;
using NexusProxy.Engine.Strategies;

namespace NexusProxy.Engine;

public sealed class LoadBalancerFactory : ILoadBalancerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LoadBalancingStrategy _strategy;

    public LoadBalancerFactory(IServiceProvider serviceProvider, IOptions<ProxyConfigOptions> options)
    {
        _serviceProvider = serviceProvider;
        _strategy = options.Value.Strategy;
    }

    public ILoadBalancer GetLoadBalancer() =>
        _strategy switch
        {
            LoadBalancingStrategy.WeightedLeastConnection =>
                _serviceProvider.GetRequiredService<WeightedLeastConnection>(),
            LoadBalancingStrategy.RoundRobin =>
                _serviceProvider.GetRequiredService<RoundRobinStrategy>(),
            _ => throw new InvalidOperationException($"Unknown load balancing strategy: {_strategy}.")
        };
}
