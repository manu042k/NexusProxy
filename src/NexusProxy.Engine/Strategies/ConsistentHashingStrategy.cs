using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NexusProxy.Core.Configuration;
using NexusProxy.Core.Interfaces;
using NexusProxy.Core.Models;

namespace NexusProxy.Engine.Strategies;

/// <summary>
/// Rendezvous (highest random weight) hashing: the same key maps to the same healthy backend;
/// when the set of healthy nodes changes, only a fraction of keys typically move compared to a simple modulo ring.
/// </summary>
public sealed class ConsistentHashingStrategy : ILoadBalancer
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<ProxyConfigOptions> _options;

    public ConsistentHashingStrategy(
        IHttpContextAccessor httpContextAccessor,
        IOptions<ProxyConfigOptions> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    public BackendServer? GetNextServer(IEnumerable<BackendServer> servers)
    {
        var healthy = servers.Where(s => s.IsHealthy).OrderBy(s => s.Name).ToList();
        if (healthy.Count == 0)
        {
            return null;
        }

        if (healthy.Count == 1)
        {
            return healthy[0];
        }

        var key = ResolveHashKey();
        BackendServer? best = null;
        var bestScore = long.MinValue;

        foreach (var server in healthy)
        {
            var score = ComputeScore(key, server.Name);
            if (best == null
                || score > bestScore
                || (score == bestScore && string.CompareOrdinal(server.Name, best.Name) < 0))
            {
                best = server;
                bestScore = score;
            }
        }

        return best;
    }

    private string ResolveHashKey()
    {
        var http = _httpContextAccessor.HttpContext;
        var headerName = _options.Value.ConsistentHashing?.KeyHeader;

        if (!string.IsNullOrWhiteSpace(headerName)
            && http != null
            && http.Request.Headers.TryGetValue(headerName.Trim(), out var headerValue))
        {
            var v = headerValue.ToString().Trim();
            if (v.Length > 0)
            {
                return v;
            }
        }

        if (http != null)
        {
            if (http.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            {
                var first = forwarded.ToString().Split(',')[0].Trim();
                if (first.Length > 0)
                {
                    return first;
                }
            }

            var ip = http.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ip))
            {
                return ip;
            }
        }

        return "unknown";
    }

    private static long ComputeScore(string key, string serverId)
    {
        var payload = Encoding.UTF8.GetBytes(key + "\u0001" + serverId);
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(payload, hash);
        return BinaryPrimitives.ReadInt64BigEndian(hash);
    }
}
