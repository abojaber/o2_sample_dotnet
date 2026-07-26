# OpenObserve OTLP Demo

A .NET 9 Web API that demonstrates exporting **logs**, **traces**, and **metrics** to OpenObserve via the OpenTelemetry Protocol (OTLP/HTTP).

## Architecture

```
┌─────────────────┐     OTLP/HTTP      ┌──────────────────┐
│   .NET 9 App    │ ──────────────────▶│   OpenObserve    │
│  (OTLP Demo)    │  traces / metrics /│  (otlp.stage...) │
│                 │       logs         │                  │
└─────────────────┘                    └──────────────────┘
```

- **Traces** → `https://otlp.stage.bpo-hq.com/v1/traces`
- **Metrics** → `https://otlp.stage.bpo-hq.com/v1/metrics`
- **Logs** → `https://otlp.stage.bpo-hq.com/v1/logs`

## OpenTelemetry Sources & Meters

| Name | Type | Registered In |
|------|------|--------------|
| `otlp-demo-service` | Trace + Meter | `HomeController` |
| `otlpDemo` | Trace + Meter | `TestTrafficController` |
| `test-otlp-demo.TestService` | Trace | `TestService` (Library) |
| `test-otlp-demo.Metrics` | Meter | `TestService` (Library) |

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/` | Returns HTML page with current time; emits logs, trace span, counter metric |
| `GET` | `/hi` | Returns HTML page; emits trace with tags, histogram latency, up-down counter |
| `GET` | `/generate-traffic?count=N` | Generates N simulated requests (max 1000) with varied logs, traces, and metrics (~15% error rate) |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- OpenObserve instance (configured at `otlp.stage.bpo-hq.com`)

## Running

```bash
dotnet run
```

The app starts at `http://localhost:5119` (HTTP profile) or `https://localhost:7083` (HTTPS profile).

## Generating Test Traffic

```bash
./generate-traffic.sh                # 10 bursts, 20 requests each
./generate-traffic.sh 50             # 50 bursts
./generate-traffic.sh 50 http://localhost:5119 30  # custom bursts, URL, requests/burst
```

## Viewing in OpenObserve

Go to OpenObserve (org: `default`) and check the streams:
- `dotnetlogs` — application logs
- `dotnettracing` — distributed traces
- `dotnetmetrics` — metrics (counters, histograms, gauges)

**Tip:** widen the time range if you see no data; the default window is narrow.

## Project Structure

```
OpenObserveDemo/
├── Controllers/
│   ├── HomeController.cs          # Root page and /hi endpoint
│   └── TestTrafficController.cs   # Synthetic traffic generator
├── Library/
│   └── TestService.cs             # Shared service with its own trace/metric
├── Properties/
│   └── launchSettings.json
├── Program.cs                     # App entry point, OTLP configuration
├── appsettings.json
├── appsettings.Development.json
├── generate-traffic.sh            # Shell script for load generation
├── OpenObserveDemo.csproj
└── OpenObserveDemo.http           # VS Code REST client file
```