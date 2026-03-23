using NexusProxy.Core.Models;
using NexusProxy.Engine.Strategies;
using NexusProxy.Tests.Unit.Helpers;

namespace NexusProxy.Tests.Unit.Strategies;

public class PowerOfTwoChoicesStrategyTests
{
    private readonly PowerOfTwoChoicesStrategy _sut = new();

    [Fact]
    public void GetNextServer_NoHealthy_ReturnsNull()
    {
        Assert.Null(_sut.GetNextServer(new[] { TestBackends.Create("x", healthy: false) }));
    }

    [Fact]
    public void GetNextServer_OneHealthy_ReturnsIt()
    {
        var only = TestBackends.Create("solo");
        Assert.Same(only, _sut.GetNextServer(new[] { only, TestBackends.Create("down", healthy: false) }));
    }

    [Fact]
    public void GetNextServer_PrefersLowerWeightedLoadAmongTwo()
    {
        var busy = TestBackends.Create("busy", weight: 1);
        var idle = TestBackends.Create("idle", weight: 1);
        for (var i = 0; i < 10; i++)
        {
            busy.IncrementActiveConnections();
        }

        var servers = new List<BackendServer> { busy, idle };
        for (var i = 0; i < 200; i++)
        {
            Assert.Equal("idle", _sut.GetNextServer(servers)!.Name);
        }
    }
}
