using NexusProxy.Core.Models;
using NexusProxy.Engine.Strategies;
using NexusProxy.Tests.Unit.Helpers;

namespace NexusProxy.Tests.Unit.Strategies;

public class RandomStrategyTests
{
    private readonly RandomStrategy _sut = new();

    [Fact]
    public void GetNextServer_NoHealthy_ReturnsNull()
    {
        var servers = new List<BackendServer> { TestBackends.Create("x", healthy: false) };
        Assert.Null(_sut.GetNextServer(servers));
    }

    [Fact]
    public void GetNextServer_SingleHealthy_AlwaysThatServer()
    {
        var servers = new List<BackendServer> { TestBackends.Create("only") };
        for (var i = 0; i < 20; i++)
        {
            Assert.Equal("only", _sut.GetNextServer(servers)!.Name);
        }
    }

    [Fact]
    public void GetNextServer_TwoHealthy_EventuallySelectsBoth()
    {
        var servers = new List<BackendServer>
        {
            TestBackends.Create("left"),
            TestBackends.Create("right")
        };

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < 800; i++)
        {
            seen.Add(_sut.GetNextServer(servers)!.Name);
            if (seen.Count == 2)
            {
                return;
            }
        }

        Assert.Fail("Expected both backends to be selected at least once.");
    }
}
