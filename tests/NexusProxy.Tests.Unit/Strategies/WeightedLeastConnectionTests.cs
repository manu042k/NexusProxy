using NexusProxy.Core.Models;
using NexusProxy.Engine.Strategies;
using NexusProxy.Tests.Unit.Helpers;

namespace NexusProxy.Tests.Unit.Strategies;

public class WeightedLeastConnectionTests
{
    private readonly WeightedLeastConnection _sut = new();

    [Fact]
    public void GetNextServer_PrefersLowerLoadPerWeight()
    {
        // Same weight: 3 active on a, 1 on b -> pick b
        var a = TestBackends.Create("a", weight: 2);
        var b = TestBackends.Create("b", weight: 2);
        a.IncrementActiveConnections();
        a.IncrementActiveConnections();
        a.IncrementActiveConnections();
        b.IncrementActiveConnections();
        var servers = new List<BackendServer> { a, b };

        Assert.Equal("b", _sut.GetNextServer(servers)!.Name);
    }

    [Fact]
    public void GetNextServer_HigherWeightAcceptsMoreConnections()
    {
        // a: 2/2 = 1, b: 2/4 = 0.5 -> pick b
        var a = TestBackends.Create("a", weight: 2);
        var b = TestBackends.Create("b", weight: 4);
        a.IncrementActiveConnections();
        a.IncrementActiveConnections();
        b.IncrementActiveConnections();
        b.IncrementActiveConnections();
        var servers = new List<BackendServer> { a, b };

        Assert.Equal("b", _sut.GetNextServer(servers)!.Name);
    }
}
