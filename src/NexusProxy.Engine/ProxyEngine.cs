using Microsoft.AspNetCore.Http;
using NexusProxy.Core.Models;

namespace NexusProxy.Engine;

public class ProxyEngine
{
    private readonly HttpClient _httpClient;

    public ProxyEngine(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ForwardRequestAsync(HttpContext context, BackendServer target, CancellationToken ct)
    {
        // 1. Create the target URL
        var targetUri = new Uri(target.Address, context.Request.Path + context.Request.QueryString);

        // 2. Create the proxy request message
        using var proxyRequest = new HttpRequestMessage();
        proxyRequest.RequestUri = targetUri;
        proxyRequest.Method = new HttpMethod(context.Request.Method);

        // 3. Copy Request Headers (excluding Hop-by-Hop headers)
        foreach (var header in context.Request.Headers)
        {
            if (!proxyRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                proxyRequest.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        // 4. Send the request to the backend
        using var response = await _httpClient.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead, ct);

        // 5. Copy Response Status and Headers back to the client
        context.Response.StatusCode = (int)response.StatusCode;
        foreach (var header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        // 6. Stream the body content back
        await response.Content.CopyToAsync(context.Response.Body, ct);
    }
}