using NexusProxy.Core.Interfaces;
using NexusProxy.Core.Models;

namespace NexusProxy.Engine.Strategies;

/// <summary>
/// Picks the healthy backend with the lowest load score: <c>ActiveConnections / max(Weight, 1)</c>.
/// Higher weight increases capacity, so more connections are acceptable before this node is chosen again.
/// </summary>
public sealed class WeightedLeastConnection : ILoadBalancer
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
            if (best == null || IsBetterCandidate(server, best))
            {
                best = server;
            }
        }

        return best;
    }

    /// <summary>
    /// Compares load as <c>ActiveConnections / max(Weight,1)</c> using exact integer math (<c>a.Active * b.Weight</c> vs <c>b.Active * a.Weight</c>).
    /// </summary>
    private static bool IsBetterCandidate(BackendServer candidate, BackendServer current)
    {
        var cw = Math.Max(1, candidate.Weight);
        var w = Math.Max(1, current.Weight);
        var left = (long)candidate.ActiveConnections * w;
        var right = (long)current.ActiveConnections * cw;
        if (left < right)
        {
            return true;
        }

        if (left > right)
        {
            return false;
        }

        return string.CompareOrdinal(candidate.Name, current.Name) < 0;
    }
}
