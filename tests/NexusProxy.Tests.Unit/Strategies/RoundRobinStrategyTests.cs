using NexusProxy.Core.Models;
using NexusProxy.Engine.Strategies;
using NexusProxy.Tests.Unit.Helpers;

namespace NexusProxy.Tests.Unit.Strategies;

public class RoundRobinStrategyTests
{
    private readonly RoundRobinStrategy _sut = new();

    [Fact]
    public void GetNextServer_NoHealthy_ReturnsNull()
    {
        var servers = new List<BackendServer>
        {
            TestBackends.Create("a", healthy: false),
            TestBackends.Create("b", healthy: false)
        };

        Assert.Null(_sut.GetNextServer(servers));
    }

    [Fact]
    public void GetNextServer_SkipsUnhealthy_CyclesHealthyInOrder()
    {
        var servers = new List<BackendServer>
        {
            TestBackends.Create("a"),
            TestBackends.Create("b", healthy: false),
            TestBackends.Create("c")
        };

        Assert.Equal("a", _sut.GetNextServer(servers)!.Name);
        Assert.Equal("c", _sut.GetNextServer(servers)!.Name);
        Assert.Equal("a", _sut.GetNextServer(servers)!.Name);
        Assert.Equal("c", _sut.GetNextServer(servers)!.Name);
    }

    [Fact]
    public void GetNextServer_ThreeHealthy_Rotates()
    {
        var servers = new List<BackendServer>
        {
            TestBackends.Create("x"),
            TestBackends.Create("y"),
            TestBackends.Create("z")
        };

        Assert.Equal("x", _sut.GetNextServer(servers)!.Name);
        Assert.Equal("y", _sut.GetNextServer(servers)!.Name);
        Assert.Equal("z", _sut.GetNextServer(servers)!.Name);
        Assert.Equal("x", _sut.GetNextServer(servers)!.Name);
    }
}
