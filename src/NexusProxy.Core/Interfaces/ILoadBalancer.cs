using NexusProxy.Core.Models;

namespace NexusProxy.Core.Interfaces;

public interface ILoadBalancer
{
    BackendServer? GetNextServer(IEnumerable<BackendServer> servers);
}