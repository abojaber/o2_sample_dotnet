# dotnet-otlp-openobserve

> [!NOTE] orginaly created by Konrad Kaminski-Pawlak
> Dotnet with OpenTelemetry + OpenObserve - 1 Web + 2 API. A sample code showing how to instrument three asp.net core 7 microservices for distributed tracing and metrics with OTLP exported directly to OpenObserve over HTTP.
> Serilog Sink for OpenObserve is an extension that integrates Serilog, a favored logging library for .NET applications, with OpenObserve. Crafted by Konrad Kaminski-Pawlak, the sink allows for effortless logging to OpenObserve, thereby enhancing the ability to store, analyze, and manage logs. See [Serilog Sink for OpenObserve](https://openobserve.ai/blog/serilog-sink-for-openobserve#introducing-serilog-sink-for-openobserve)

## Requirenments

Docker with OpenObserve
See [https://openobserve.ai/docs/quickstart/#openobserve-cloud](https://openobserve.ai/docs/quickstart/#openobserve-cloud)

Run with

```
mkdir data
docker run -v $PWD/data:/data -e ZO_DATA_DIR="/data" -p 5080:5080 \
    -e ZO_ROOT_USER_EMAIL="root@example.com" -e ZO_ROOT_USER_PASSWORD="Complexpass#123" \
    public.ecr.aws/zinclabs/openobserve:latest
```

## Project dotnet

Set multiple Start Visual Studio - Web, Api1, Api2
Will start the https profile configs

### Configurations

- ApiApplication1 (calls api2)

```
  "Api2Url": "https://localhost:7132",
  "OpenObserve": {
    "OtlpEndpoint": "http://localhost:5080",
    "Organization": "default",
    "Login": "root@admin.com",
    "Key": "qwe123QWE"
  }
```

- ApiApplication2 (standalone)

```
  "OpenObserve": {
    "OtlpEndpoint": "http://localhost:5080",
    "Organization": "default",
    "Login": "root@admin.com",
    "Key": "qwe123QWE"
  }
```

- WebApplication1 (calls api1 & api2)

```
  "OpenObserve": {
    "OtlpEndpoint": "http://localhost:5080",
    "Organization": "default",
    "Login": "root@admin.com",
    "Key": "qwe123QWE"
  },

  "Api1Url": "https://localhost:7199",
  "Api2Url": "https://localhost:7132"
```

## DEMO

### ServiceMap

![ServiceMap .](/assets/images/DEMO-servicemap.png)

### Trace Timeline

![trace-timeline](/assets/images/DEMO-trace-timeline.png)

### Discover

![discover.](/assets/images/DEMO-discover.png)

## DEMO Log Exception

### Trace Timeline

![trace-timeline](/assets/images/DEMO-trace-timeline-exception.png)

### Observability To Discover

![Observability To Discover.](/assets/images/DEMO-observabilityToDiscover.png)

## OTLP Endpoints

OpenTelemetry traces and metrics are exported directly to OpenObserve's native OTLP HTTP endpoints:

| Signal  | Endpoint                                       |
| ------- | ---------------------------------------------- |
| Traces  | `http://localhost:5080/api/default/v1/traces`  |
| Metrics | `http://localhost:5080/api/default/v1/metrics` |

Authentication is sent via `Authorization: Basic` header using the configured credentials.

See `SharedLib/OpenTelemetryToELKStack.cs` for the OTLP setup.
