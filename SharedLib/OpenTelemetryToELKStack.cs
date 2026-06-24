using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Text;

namespace SharedLib
{
    public static class OpenTelemetryToELKStack
    {
        public static void AddOpenTelemetryToELKStack(this WebApplicationBuilder builder, string servicename, string otlpBaseUrl, string? login = null, string? key = null, string organization = "default")
        {
            var authHeader = !string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(key)
                ? $"Authorization=Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{key}"))}"
                : null;

            var tracesEndpoint = $"{otlpBaseUrl}/api/{organization}/v1/traces";
            var metricsEndpoint = $"{otlpBaseUrl}/api/{organization}/v1/metrics";

            var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName: servicename);

            Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(new TextMapPropagator[]
            {
                new TraceContextPropagator(),
                new BaggagePropagator()
            }));

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(builder => builder.AddService(serviceName: servicename))
                .WithTracing(builder => builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(configure =>
                    {
                        configure.Endpoint = new Uri(tracesEndpoint);
                        configure.Protocol = OtlpExportProtocol.HttpProtobuf;
                        if (authHeader != null)
                            configure.Headers = authHeader;
                    })
                )
                .WithMetrics(builder => builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(configure =>
                    {
                        configure.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                        configure.Endpoint = new Uri(metricsEndpoint);
                        if (authHeader != null)
                            configure.Headers = authHeader;
                    })
                );
        }
   }
}