using NexusProxy.Core.Models;
using NexusProxy.Engine.Strategies;
using NexusProxy.Tests.Unit.Helpers;

namespace NexusProxy.Tests.Unit.Strategies;

public class LeastConnectionsStrategyTests
{
    private readonly LeastConnectionsStrategy _sut = new();

    [Fact]
    public void GetNextServer_PrefersFewerActiveConnections_TieBreaksByName()
    {
        var a = TestBackends.Create("a");
        var b = TestBackends.Create("b");
        a.IncrementActiveConnections();
        a.IncrementActiveConnections();
        var servers = new List<BackendServer> { a, b };

        Assert.Equal("b", _sut.GetNextServer(servers)!.Name);
    }

    [Fact]
    public void GetNextServer_SameConnections_PrefersLexicographicallySmallerName()
    {
        var servers = new List<BackendServer>
        {
            TestBackends.Create("zebra"),
            TestBackends.Create("alpha")
        };

        Assert.Equal("alpha", _sut.GetNextServer(servers)!.Name);
    }

    [Fact]
    public void GetNextServer_IgnoresWeight()
    {
        var heavy = TestBackends.Create("heavy", weight: 10);
        var light = TestBackends.Create("light", weight: 1);
        heavy.IncrementActiveConnections();
        var servers = new List<BackendServer> { heavy, light };

        Assert.Equal("light", _sut.GetNextServer(servers)!.Name);
    }
}
