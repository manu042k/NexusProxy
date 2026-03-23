using NexusProxy.Core.Models;
using NexusProxy.Engine.Strategies;
using NexusProxy.Tests.Unit.Helpers;

namespace NexusProxy.Tests.Unit.Strategies;

public class WeightedRoundRobinStrategyTests
{
    [Fact]
    public void GetNextServer_OverManyRequests_RespectsWeightRatio()
    {
        var sut = new WeightedRoundRobinStrategy();
        var servers = new List<BackendServer>
        {
            TestBackends.Create("a", weight: 2),
            TestBackends.Create("b", weight: 1)
        };

        var counts = new Dictionary<string, int>(StringComparer.Ordinal) { ["a"] = 0, ["b"] = 0 };
        const int iterations = 3000;
        for (var i = 0; i < iterations; i++)
        {
            var next = sut.GetNextServer(servers)!.Name;
            counts[next]++;
        }

        var ratio = counts["a"] / (double)counts["b"];
        Assert.InRange(ratio, 1.85, 2.15);
    }
}
