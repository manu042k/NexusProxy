using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NexusProxy.Core.Configuration;
using NexusProxy.Core.Models;
using NexusProxy.Engine.Strategies;
using NexusProxy.Tests.Unit.Helpers;

namespace NexusProxy.Tests.Unit.Strategies;

public class ConsistentHashingStrategyTests
{
    private sealed class TestAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private static ConsistentHashingStrategy CreateSut(
        HttpContext? http,
        ProxyConfigOptions? proxyOptions = null)
    {
        var accessor = new TestAccessor { HttpContext = http };
        var options = Options.Create(proxyOptions ?? new ProxyConfigOptions());
        return new ConsistentHashingStrategy(accessor, options);
    }

    private static List<BackendServer> TwoServers() =>
        new()
        {
            TestBackends.Create("node-a"),
            TestBackends.Create("node-b")
        };

    [Fact]
    public void GetNextServer_SameKey_SelectsSameBackendRepeatedly()
    {
        var ctx = new DefaultHttpContext();
        ctx.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.50");
        var sut = CreateSut(ctx);
        var servers = TwoServers();

        var first = sut.GetNextServer(servers)!.Name;
        Assert.Equal(first, sut.GetNextServer(servers)!.Name);
        Assert.Equal(first, sut.GetNextServer(servers)!.Name);
    }

    [Fact]
    public void GetNextServer_KeyHeader_WinsOverForwardedForAndIp()
    {
        var ctx = new DefaultHttpContext();
        ctx.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        ctx.Request.Headers["X-Forwarded-For"] = "198.51.100.1";
        ctx.Request.Headers["X-Tenant"] = "tenant-fixed";

        var sut = CreateSut(ctx, new ProxyConfigOptions
        {
            ConsistentHashing = new ConsistentHashingOptions { KeyHeader = "X-Tenant" }
        });
        var servers = TwoServers();

        var pick = sut.GetNextServer(servers)!.Name;
        Assert.Equal(pick, sut.GetNextServer(servers)!.Name);
    }

    [Fact]
    public void GetNextServer_UsesForwardedFor_WhenNoKeyHeader()
    {
        var ctx = new DefaultHttpContext();
        ctx.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        ctx.Request.Headers["X-Forwarded-For"] = "198.51.100.2, 10.0.0.1";

        var sut = CreateSut(ctx);
        var servers = TwoServers();

        var pick = sut.GetNextServer(servers)!.Name;
        Assert.Equal(pick, sut.GetNextServer(servers)!.Name);
    }

    [Fact]
    public void GetNextServer_NoHttpContext_UsesUnknownKey_StillDeterministic()
    {
        var sut = CreateSut(null);
        var servers = TwoServers();

        var pick = sut.GetNextServer(servers)!.Name;
        Assert.Equal(pick, sut.GetNextServer(servers)!.Name);
    }
}
