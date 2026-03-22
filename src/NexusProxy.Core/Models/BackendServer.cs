namespace NexusProxy.Core.Models;

public class BackendServer
{
    public required string Name { get; set; }
    public required Uri Address { get; set; }
    public bool IsHealthy { get; set; } = true;
    public int ActiveConnections { get; set; }
    public int Weight { get; set; } = 1;
}