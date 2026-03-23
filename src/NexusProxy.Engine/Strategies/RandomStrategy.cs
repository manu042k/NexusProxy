using NexusProxy.Core.Interfaces;
using NexusProxy.Core.Models;

namespace NexusProxy.Engine.Strategies;

/// <summary>
/// Uniformly picks a random healthy backend. Stateless and cheap; ignores <see cref="BackendServer.Weight"/>.
/// </summary>
public sealed class RandomStrategy : ILoadBalancer
{
    public BackendServer? GetNextServer(IEnumerable<BackendServer> servers)
    {
        var healthy = servers.Where(s => s.IsHealthy).ToList();
        if (healthy.Count == 0)
        {
            return null;
        }

        return healthy[Random.Shared.Next(healthy.Count)];
    }
}
