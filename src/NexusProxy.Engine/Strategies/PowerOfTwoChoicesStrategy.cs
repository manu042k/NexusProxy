using NexusProxy.Core.Interfaces;
using NexusProxy.Core.Models;

namespace NexusProxy.Engine.Strategies;

/// <summary>
/// Chooses two random healthy backends and sends to the one with lower weighted load
/// (<c>ActiveConnections / max(Weight,1)</c>). Cheaper than scanning all nodes at very large pool sizes.
/// </summary>
public sealed class PowerOfTwoChoicesStrategy : ILoadBalancer
{
    public BackendServer? GetNextServer(IEnumerable<BackendServer> servers)
    {
        var healthy = servers.Where(s => s.IsHealthy).ToList();
        if (healthy.Count == 0)
        {
            return null;
        }

        if (healthy.Count == 1)
        {
            return healthy[0];
        }

        var i = Random.Shared.Next(healthy.Count);
        var j = Random.Shared.Next(healthy.Count - 1);
        if (j >= i)
        {
            j++;
        }

        var a = healthy[i];
        var b = healthy[j];
        return BetterByWeightedLoad(a, b);
    }

    private static BackendServer BetterByWeightedLoad(BackendServer a, BackendServer b)
    {
        var aw = Math.Max(1, a.Weight);
        var bw = Math.Max(1, b.Weight);
        var left = (long)a.ActiveConnections * bw;
        var right = (long)b.ActiveConnections * aw;
        if (left < right)
        {
            return a;
        }

        if (left > right)
        {
            return b;
        }

        return string.CompareOrdinal(a.Name, b.Name) <= 0 ? a : b;
    }
}
