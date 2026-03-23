using Microsoft.Extensions.Options;
using NexusProxy.Core.Configuration;
using NexusProxy.Core.Interfaces;
using NexusProxy.Core.Models;
using NexusProxy.Engine;
using NexusProxy.Engine.Middleware;
using NexusProxy.Engine.Strategies;

var builder = WebApplication.CreateBuilder(args);

// 1. Proxy configuration (strategy + backends)
builder.Services.Configure<ProxyConfigOptions>(
    builder.Configuration.GetSection(ProxyConfigOptions.SectionName));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ProxyConfigOptions>>().Value.Backends);

// 2. Register Services (Dependency Injection)
builder.Services.AddSingleton<RoundRobinStrategy>();
builder.Services.AddSingleton<WeightedLeastConnection>();
builder.Services.AddSingleton<RandomStrategy>();
builder.Services.AddSingleton<LeastConnectionsStrategy>();
builder.Services.AddSingleton<WeightedRoundRobinStrategy>();
builder.Services.AddSingleton<PowerOfTwoChoicesStrategy>();
builder.Services.AddSingleton<ILoadBalancerFactory, LoadBalancerFactory>();
builder.Services.AddHttpClient<ProxyEngine>()
    .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
    {
        var handler = new HttpClientHandler();
        var env = serviceProvider.GetRequiredService<IHostEnvironment>();
        // Development only: corporate proxies / antivirus HTTPS inspection often break default TLS validation.
        if (env.IsDevelopment())
        {
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        return handler;
    });

var app = builder.Build();

// 3. Setup Middleware Pipeline
// Add our custom logging first to catch everything
app.UseMiddleware<RequestLoggingMiddleware>();

// 4. The Proxy Logic (Catch-all Route)
app.Map("{*path}", async (
    HttpContext context,
    ProxyEngine engine,
    ILoadBalancerFactory loadBalancerFactory,
    List<BackendServer> servers,
    CancellationToken ct) =>
{
    var loadBalancer = loadBalancerFactory.GetLoadBalancer();
    var target = loadBalancer.GetNextServer(servers);

    if (target == null)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsync("No healthy backend servers available.");
        return;
    }

    target.IncrementActiveConnections();
    try
    {
        await engine.ForwardRequestAsync(context, target, ct);
    }
    catch (Exception ex)
    {
        target.IsHealthy = false;
        context.Response.StatusCode = StatusCodes.Status502BadGateway;
        await context.Response.WriteAsync($"Error contacting backend: {ex.Message}");
    }
    finally
    {
        target.DecrementActiveConnections();
    }
});

app.Run();