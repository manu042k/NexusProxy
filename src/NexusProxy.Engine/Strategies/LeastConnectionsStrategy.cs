using NexusProxy.Core.Interfaces;
using NexusProxy.Core.Models;

namespace NexusProxy.Engine.Strategies;

/// <summary>
/// Picks the healthy backend with the fewest <see cref="BackendServer.ActiveConnections"/>.
/// Tie-break: <see cref="BackendServer.Name"/> (ordinal). Ignores <see cref="BackendServer.Weight"/>.
/// </summary>
public sealed class LeastConnectionsStrategy : ILoadBalancer
{
    public BackendServer? GetNextServer(IEnumerable<BackendServer> servers)
    {
        var healthy = servers.Where(s => s.IsHealthy).ToList();
        if (healthy.Count == 0)
        {
            return null;
        }

        BackendServer? best = null;
        foreach (var server in healthy)
        {
            if (best == null
                || server.ActiveConnections < best.ActiveConnections
                || (server.ActiveConnections == best.ActiveConnections
                    && string.CompareOrdinal(server.Name, best.Name) < 0))
            {
                best = server;
            }
        }

        return best;
    }
}
