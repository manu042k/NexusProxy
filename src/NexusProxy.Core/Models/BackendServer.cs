namespace NexusProxy.Core.Models;

public class BackendServer
{
    private int _activeConnections;

    public required string Name { get; set; }
    public required Uri Address { get; set; }
    public bool IsHealthy { get; set; } = true;

    /// <summary>Current in-flight proxied requests (thread-safe).</summary>
    public int ActiveConnections => Volatile.Read(ref _activeConnections);

    public int Weight { get; set; } = 1;

    public void IncrementActiveConnections() => Interlocked.Increment(ref _activeConnections);

    public void DecrementActiveConnections() => Interlocked.Decrement(ref _activeConnections);
}