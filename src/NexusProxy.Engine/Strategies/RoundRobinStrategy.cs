using NexusProxy.Core.Interfaces;
using NexusProxy.Core.Models;
using System.Collections.Concurrent;

namespace NexusProxy.Engine.Strategies;

public class RoundRobinStrategy : ILoadBalancer{
    private readonly ConcurrentDictionary<string, int> _lastIndex = new();

    public BackendServer? GetNextServer(IEnumerable<BackendServer> servers){

        var healthyServers = servers.Where(s => s.IsHealthy).ToList();
        if (healthyServers.Count == 0)
        {
            return null;
        }

        var nextIndex = _lastIndex.AddOrUpdate(
            "default",
            0,
            (_, val) => (val + 1) % healthyServers.Count);

        return healthyServers[nextIndex % healthyServers.Count];
    }
}