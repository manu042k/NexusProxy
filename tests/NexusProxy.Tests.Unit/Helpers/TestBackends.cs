using NexusProxy.Core.Models;

namespace NexusProxy.Tests.Unit.Helpers;

internal static class TestBackends
{
    public static BackendServer Create(
        string name,
        string address = "https://example.com/",
        int weight = 1,
        bool healthy = true) =>
        new()
        {
            Name = name,
            Address = new Uri(address),
            Weight = weight,
            IsHealthy = healthy
        };
}
