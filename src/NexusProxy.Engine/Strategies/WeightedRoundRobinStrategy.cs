using NexusProxy.Core.Interfaces;
using NexusProxy.Core.Models;

namespace NexusProxy.Engine.Strategies;

/// <summary>
/// Smooth weighted round robin: over time, each server receives traffic in proportion to <see cref="BackendServer.Weight"/>.
/// Uses per-server current-weight state (keyed by <see cref="BackendServer.Name"/>).
/// </summary>
public sealed class WeightedRoundRobinStrategy : ILoadBalancer
{
    private readonly Dictionary<string, int> _currentWeights = new();
    private readonly object _sync = new();

    public BackendServer? GetNextServer(IEnumerable<BackendServer> servers)
    {
        var healthy = servers.Where(s => s.IsHealthy).OrderBy(s => s.Name).ToList();
        if (healthy.Count == 0)
        {
            return null;
        }

        var totalWeight = healthy.Sum(s => Math.Max(1, s.Weight));

        lock (_sync)
        {
            BackendServer? best = null;
            var bestCw = int.MinValue;

            foreach (var server in healthy)
            {
                var w = Math.Max(1, server.Weight);
                _currentWeights.TryGetValue(server.Name, out var prev);
                var cw = prev + w;
                _currentWeights[server.Name] = cw;

                if (cw > bestCw
                    || (cw == bestCw && best != null && string.CompareOrdinal(server.Name, best.Name) < 0))
                {
                    best = server;
                    bestCw = cw;
                }
            }

            if (best != null)
            {
                _currentWeights[best.Name] -= totalWeight;
            }

            return best;
        }
    }
}
