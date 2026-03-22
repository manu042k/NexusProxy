using NexusProxy.Core.Models;

namespace NexusProxy.Core.Interfaces;

public interface IHealthCheckService
{
    Task CheckHealthAsync(IEnumerable<BackendServer> servers, CancellationToken ct);
}