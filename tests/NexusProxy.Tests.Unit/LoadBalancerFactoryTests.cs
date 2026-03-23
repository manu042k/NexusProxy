using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NexusProxy.Core.Configuration;
using NexusProxy.Core.Interfaces;
using NexusProxy.Engine;
using NexusProxy.Engine.Strategies;

namespace NexusProxy.Tests.Unit;

public class LoadBalancerFactoryTests
{
    private static ServiceProvider BuildProvider(LoadBalancingStrategy strategy)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.Configure<ProxyConfigOptions>(o => o.Strategy = strategy);
        services.AddSingleton<RoundRobinStrategy>();
        services.AddSingleton<WeightedLeastConnection>();
        services.AddSingleton<RandomStrategy>();
        services.AddSingleton<LeastConnectionsStrategy>();
        services.AddSingleton<WeightedRoundRobinStrategy>();
        services.AddSingleton<PowerOfTwoChoicesStrategy>();
        services.AddSingleton<ConsistentHashingStrategy>();
        services.AddSingleton<ILoadBalancerFactory, LoadBalancerFactory>();
        return services.BuildServiceProvider();
    }

    public static TheoryData<LoadBalancingStrategy, Type> StrategyTypes =>
        new()
        {
            { LoadBalancingStrategy.RoundRobin, typeof(RoundRobinStrategy) },
            { LoadBalancingStrategy.WeightedLeastConnection, typeof(WeightedLeastConnection) },
            { LoadBalancingStrategy.Random, typeof(RandomStrategy) },
            { LoadBalancingStrategy.LeastConnections, typeof(LeastConnectionsStrategy) },
            { LoadBalancingStrategy.WeightedRoundRobin, typeof(WeightedRoundRobinStrategy) },
            { LoadBalancingStrategy.PowerOfTwoChoices, typeof(PowerOfTwoChoicesStrategy) },
            { LoadBalancingStrategy.ConsistentHashing, typeof(ConsistentHashingStrategy) }
        };

    [Theory]
    [MemberData(nameof(StrategyTypes))]
    public void GetLoadBalancer_ReturnsStrategyImplementation(LoadBalancingStrategy configured, Type expectedType)
    {
        using var provider = BuildProvider(configured);
        var factory = provider.GetRequiredService<ILoadBalancerFactory>();
        var balancer = factory.GetLoadBalancer();
        Assert.IsType(expectedType, balancer);
    }
}
