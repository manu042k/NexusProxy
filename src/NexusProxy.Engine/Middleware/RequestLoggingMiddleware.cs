using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NexusProxy.Engine.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start a high-resolution timer
        var sw = Stopwatch.StartNew();

        try
        {
            // Call the next component in the pipeline (e.g., the ProxyEngine)
            await _next(context);
            
            sw.Stop();

            // Log successful proxying
            _logger.LogInformation(
                "Proxy Request: {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Proxy Request Failed: {Method} {Path} after {Elapsed}ms", 
                context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
            
            // Re-throw so the global exception handler can catch it
            throw;
        }
    }
}