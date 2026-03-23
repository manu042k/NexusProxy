using NexusProxy.Core.Interfaces;
using NexusProxy.Core.Models;
using NexusProxy.Engine;
using NexusProxy.Engine.Middleware;
using NexusProxy.Engine.Strategies;

var builder = WebApplication.CreateBuilder(args);

// 1. Load Backend Configuration
var backendConfigs = builder.Configuration
    .GetSection("ProxyConfig:Backends")
    .Get<List<BackendServer>>() ?? new List<BackendServer>();

// 2. Register Services (Dependency Injection)
builder.Services.AddSingleton(backendConfigs);
builder.Services.AddSingleton<ILoadBalancer, RoundRobinStrategy>();
builder.Services.AddHttpClient<ProxyEngine>();

var app = builder.Build();

// 3. Setup Middleware Pipeline
// Add our custom logging first to catch everything
app.UseMiddleware<RequestLoggingMiddleware>();

// 4. The Proxy Logic (Catch-all Route)
app.Map("{*path}", async (
    HttpContext context, 
    ProxyEngine engine, 
    ILoadBalancer loadBalancer, 
    List<BackendServer> servers,
    CancellationToken ct) =>
{
    // Pick a server using our strategy
    var target = loadBalancer.GetNextServer(servers);

    if (target == null)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsync("No healthy backend servers available.");
        return;
    }

    // Forward the request
    try 
    {
        await engine.ForwardRequestAsync(context, target, ct);
    }
    catch (Exception ex)
    {
        // Handle cases where the backend is down
        target.IsHealthy = false; 
        context.Response.StatusCode = StatusCodes.Status502BadGateway;
        await context.Response.WriteAsync($"Error contacting backend: {ex.Message}");
    }
});

app.Run();