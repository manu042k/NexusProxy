namespace NexusProxy.Core.Configuration;

/// <summary>Settings for <see cref="LoadBalancingStrategy.ConsistentHashing"/>.</summary>
public class ConsistentHashingOptions
{
    /// <summary>
    /// If set, the first value of this request header is the hash key (after trim).
    /// If unset or empty, the key is the client address: first hop of <c>X-Forwarded-For</c> when present, otherwise the connection remote IP.
    /// </summary>
    public string? KeyHeader { get; set; }
}
