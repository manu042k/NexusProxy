# NexusProxy

A minimal ASP.NET Core reverse proxy with **configurable load-balancing strategies**. Incoming HTTP requests are forwarded to one of several backend origins; the strategy decides which healthy backend handles each request.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or compatible with the `TargetFramework` in the `.csproj` files)

## Solution layout

| Project                   | Role                                                                    |
| ------------------------- | ----------------------------------------------------------------------- |
| **NexusProxy.Server**     | ASP.NET Core host, configuration, HTTP pipeline                         |
| **NexusProxy.Engine**     | Proxy forwarding, middleware, load-balancer implementations and factory |
| **NexusProxy.Core**       | Shared models, options, and interfaces                                  |
| **NexusProxy.Tests.Unit** | Unit tests                                                              |

## Build, run, and test

```bash
dotnet build NexusProxy.sln
dotnet test NexusProxy.sln
```

Run the server (from the repository root or the server project folder):

```bash
dotnet run --project src/NexusProxy.Server
```

By default the app listens using [launchSettings](src/NexusProxy.Server/Properties/launchSettings.json). Override the URL if needed:

```bash
dotnet run --project src/NexusProxy.Server --urls "http://127.0.0.1:5080"
```

Send traffic through the proxy:

```bash
curl -i http://127.0.0.1:5080/some/path
```

The proxy combines each backend’s **base** `Address` with the incoming path and query string.

## Configuration

Settings live under `ProxyConfig` in `appsettings.json` (and environment-specific files such as `appsettings.Development.json`).

### Example

```json
"ProxyConfig": {
  "Strategy": "RoundRobin",
  "Backends": [
    { "Name": "api-1", "Address": "https://api.example.com/", "Weight": 2 },
    { "Name": "api-2", "Address": "https://backup.example.com/", "Weight": 1 }
  ]
}
```

### Backend fields

| Property    | Description                                                                                              |
| ----------- | -------------------------------------------------------------------------------------------------------- |
| `Name`      | Stable identifier (used for ordering and weighted round-robin state).                                    |
| `Address`   | Base URI of the origin (prefer a trailing slash on the authority path, e.g. `https://host/`).            |
| `Weight`    | Relative capacity for weighted strategies (minimum effective weight is `1`).                             |
| `IsHealthy` | Set at runtime; unhealthy nodes are skipped until the process is restarted or you extend recovery logic. |

### Load-balancing strategies

Set `Strategy` to one of the following (string names are case-insensitive when bound from configuration):

| Strategy                  | Description                                                                                                                                                     |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `RoundRobin`              | Cycles across healthy backends in order.                                                                                                                        |
| `WeightedLeastConnection` | Minimizes `ActiveConnections / max(Weight, 1)` among healthy backends.                                                                                          |
| `LeastConnections`        | Chooses the fewest active connections; ignores `Weight`.                                                                                                        |
| `WeightedRoundRobin`      | Smooth weighted distribution over time by `Weight`.                                                                                                             |
| `Random`                  | Uniform random choice among healthy backends; ignores `Weight`.                                                                                                 |
| `PowerOfTwoChoices`       | Picks two random healthy backends, then the lower weighted load (same score idea as weighted least connections).                                                |
| `ConsistentHashing`       | **Rendezvous hashing**: same hash key → same backend; stable when backends change. Key = optional header, else `X-Forwarded-For` first hop, else connection IP. |

### Consistent hashing options

When using `ConsistentHashing`, you can set `ProxyConfig:ConsistentHashing:KeyHeader` to pin tenants (or sessions) by header instead of IP:

```json
"ProxyConfig": {
  "Strategy": "ConsistentHashing",
  "ConsistentHashing": {
    "KeyHeader": "X-Tenant-Id"
  },
  "Backends": [ ... ]
}
```
