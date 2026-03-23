using NexusProxy.Core.Models;

namespace NexusProxy.Core.Configuration;

public enum LoadBalancingStrategy
{
    RoundRobin,
    WeightedLeastConnection,
    Random,
    LeastConnections,
    WeightedRoundRobin,
    PowerOfTwoChoices
}

public class ProxyConfigOptions
{
    public const string SectionName = "ProxyConfig";

    public LoadBalancingStrategy Strategy { get; set; } = LoadBalancingStrategy.RoundRobin;

    public List<BackendServer> Backends { get; set; } = new();
}
